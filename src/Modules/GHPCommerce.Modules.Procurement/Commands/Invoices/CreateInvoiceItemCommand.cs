using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using GHPCommerce.Core.Shared.Contracts.SupplierInvoices.Queries;
using GHPCommerce.Core.Shared.Services;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Infra.OS;
using GHPCommerce.Modules.Procurement.Entities;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace GHPCommerce.Modules.Procurement.Commands.Invoices
{
    public class InvoiceCreated
    {
        public ValidationResult? ValidationResult { get; set; }
        public string InternalBatch { get; set; }
    }
    public class CreateInvoiceItemCommand : ICommand<InvoiceCreated>
    {
        public Guid InvoiceId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public int Quantity { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string VendorBatchNumber { get; set; }
        public string InternalBatchNumber { get; set; }
        public decimal TotalInlTax { get; set; }
        public decimal TotalExlTax { get; set; }
        public string PackagingCode { get; set; }
        public Guid? PickingZoneId { get; set; }
        public string PickingZoneName { get; set; }
        public int Packing { get; set; }
        public decimal PFS { get; set; }//SHP 0 1.5 2.5 Prop produit 
        public decimal PpaHT { get; set; }//=PpaTTC/(1+TVA Prod) e.g :TVA= 19% => PpaHT=PpaTTC /(1.19)
        public decimal PpaTTC { get; set; }
        public decimal PpaPFS { get; set; }
        /// <summary>
        /// prix de vente
        /// </summary>
        public decimal SalePrice { get; set; }
        /// <summary>
        /// marge fournisseur
        /// </summary>
        public decimal WholesaleMargin { get; set; }

        /// <summary>
        /// marge pharmacien
        /// </summary>
        public decimal PharmacistMargin { get; set; }

        /// <summary>
        /// quantité facturée
        /// </summary>
        public int InvoicedQuantity { get; set; }

        /// <summary>
        /// quantité reçue
        /// </summary>
        public int ReceivedQuantity { get; set; }

        /// <summary>
        /// Quantité restante
        /// </summary>
        public int RemainingQuantity { get; set; }

        #region Info Achat
/// <summary>
/// prix d'achat non remisé
/// </summary>
        public decimal PurchaseUnitPrice { get; set; }
        /// <summary>
        /// prix achat remisé
        /// </summary>
        public decimal PurchasePriceIncDiscount { get; set; }
        /// <summary>
        /// taux de remise achat
        /// </summary>
        public double Discount { get; set; }

        public string InvoiceNumber { get; set; }
        public Guid OrderId { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string DocumentRef { get; set; }
        public Guid SupplierOrganizationId { get; set; }
        public decimal ReceiptsAmount { get; set; }

        #endregion
        /// <summary>
        /// gets or sets supplier id
        /// </summary>
        public Guid SupplierId { get; set; }

        /// <summary>
        /// gets or sets supplier name
        /// </summary>
        public string SupplierName { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalAmountExlTax { get; set; }

    }

    public class CreateInvoiceItemCommandValidator : AbstractValidator<CreateInvoiceItemCommand>
    {
        public CreateInvoiceItemCommandValidator()
        {
            RuleFor(v => v.ProductCode)
                .MaximumLength(200).
                NotEmpty();
            RuleFor(v => v.DocumentRef)
                .MaximumLength(200).
                NotEmpty();
            RuleFor(v => v.VendorBatchNumber)
                .MaximumLength(200).
                NotEmpty();
            RuleFor(v => v.ProductId)
                .Must(x => x != Guid.Empty);
            RuleFor(v => v.OrderId)
                .Must(x => x != Guid.Empty);
            RuleFor(v => v.InvoiceId)
                .Must(x => x != Guid.Empty);
            RuleFor(v => v.InvoiceDate)
                .Must(x => x != default);
            RuleFor(v => v.Quantity)
                .GreaterThan(0);
          
        }
    }
    public class CreateInvoiceItemCommandHandler : ICommandHandler<CreateInvoiceItemCommand, InvoiceCreated>
    {
        private readonly IRepository<SupplierInvoice, Guid> _repository;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly IMapper _mapper;
        private readonly Logger _logger;
        private readonly ISequenceNumberService<SupplierInvoice, Guid> _sequenceNumberService;
        private readonly ICommandBus _commandBus;
        public CreateInvoiceItemCommandHandler(IRepository<SupplierInvoice, Guid> repository, 
            ICurrentOrganization currentOrganization,
            IMapper mapper,
            Logger logger,
            ISequenceNumberService<SupplierInvoice, Guid> sequenceNumberService,
            ICommandBus commandBus
            )
        {
            _repository = repository;
            _currentOrganization = currentOrganization;
            _mapper = mapper;
            _logger = logger;
            _sequenceNumberService = sequenceNumberService;
            _commandBus = commandBus;
        }

        public async Task<InvoiceCreated> Handle(CreateInvoiceItemCommand request, CancellationToken cancellationToken)
        {
            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            var customerName = await _currentOrganization.GetCurrentOrganizationNameAsync();
            if (orgId == default) throw new InvalidOperationException("not allowed resources");
            var keysq = nameof(SupplierInvoice) + orgId;
            bool newInvoice = false;
            try
            {
                ValidationResult validations = default;
                var validator = new CreateInvoiceItemCommandValidator();
                var validationErrors = await validator.ValidateAsync(request, cancellationToken);
                if (!validationErrors.IsValid)
                    return new InvoiceCreated { ValidationResult = validationErrors };
                var existedInvoice = await _repository.Table.FirstOrDefaultAsync(x =>
                        x.RefDocument == request.DocumentRef && x.InvoiceDate.Date == request.InvoiceDate.Date
                                                             && x.SupplierId == request.SupplierId,
                    cancellationToken: cancellationToken);
                if (existedInvoice != null && existedInvoice.Id != request.InvoiceId)
                {
                    validations = new ValidationResult();
                    validations.Errors.Add(new ValidationFailure("Erreur de validation",
                        "Une facture avec la même  référence existe déjà dans la base de données"));
                    return new InvoiceCreated { ValidationResult = validations };
                }

                var invoice = await _repository.Table
                    .Include(x => x.Items)
                    .FirstOrDefaultAsync(x => x.Id == request.InvoiceId, cancellationToken: cancellationToken);
                if (invoice == null)
                {
                    invoice = new SupplierInvoice();
                    invoice.Id = request.InvoiceId;
                    invoice.OrderId = request.OrderId;
                    invoice.RefDocument = request.DocumentRef;
                    invoice.CustomerId = orgId.Value;
                    invoice.OrganizationId = orgId.Value;
                    invoice.SupplierId = request.SupplierOrganizationId;
                    invoice.SupplierName = request.SupplierName;
                    invoice.CustomerName = customerName;
                    invoice.InvoiceDate = request.InvoiceDate;
                    invoice.ReceiptsAmount = request.ReceiptsAmount;
                    invoice.InvoiceStatus = InvoiceStatus.Created;
                    invoice.InvoiceNumber = request.InvoiceNumber;
                    invoice.TotalAmount = request.TotalAmount;
                    invoice.TotalAmountExlTax = request.TotalAmountExlTax;
                    invoice.InvoiceDate = request.InvoiceDate;
                    newInvoice = true;
                    invoice.Items = new List<SupplierInvoiceItem>();
                    await LockProvider<string>.WaitAsync(keysq, cancellationToken);
                    var sq = await _sequenceNumberService.GenerateSequenceNumberAsync(invoice.InvoiceDate, orgId.Value);

                    invoice.SequenceNumber = sq;
                }
                else newInvoice = false;

                invoice.TotalAmount = request.TotalAmount;
                invoice.TotalAmountExlTax = request.TotalAmountExlTax;
                invoice.InvoiceDate = request.InvoiceDate;
                invoice.RefDocument = request.DocumentRef;
                var item = _mapper.Map<SupplierInvoiceItem>(request);
                item.InternalBatchNumber = await GetInternalBatchNumber(request, orgId.Value);
                invoice.Items.Add(item);
                if (newInvoice) _repository.Add(invoice);
                else _repository.Update(invoice);
                await _repository.UnitOfWork.SaveChangesAsync();
                return new InvoiceCreated() { ValidationResult = null, InternalBatch = item.InternalBatchNumber };
            }
            catch (Exception ex)
            {
                var validations = new ValidationResult
                    { Errors = { new ValidationFailure("Transaction rolled back", ex.Message) } };
                _logger.Error(ex.Message);
                _logger.Error(ex.InnerException?.Message);
                return new InvoiceCreated() { ValidationResult = validations };
            }
            finally
            {
                if(newInvoice) LockProvider<string>.Release(keysq);
            }
        }

        private async Task<string> GetInternalBatchNumber(CreateInvoiceItemCommand request, Guid orgId)
        {
            var internalBatchNumber = "";
            var query =await (from q in _repository.Table
                    from invoiceItem in q.Items
                    where invoiceItem.Packing == request.Packing
                          && Math.Abs(invoiceItem.Discount - request.Discount) <= 0
                          && invoiceItem.PurchaseUnitPrice == request.PurchaseUnitPrice
                          && invoiceItem.SalePrice == request.SalePrice
                          && invoiceItem.VendorBatchNumber == request.VendorBatchNumber
                          && q.SupplierId == request.SupplierId
                          && invoiceItem.PpaHT == request.PpaHT
                          && invoiceItem.PFS == request.PFS
                          &&q.OrganizationId == orgId
                          && invoiceItem.ExpiryDate == request.ExpiryDate 
                    select invoiceItem.InternalBatchNumber)
                .FirstOrDefaultAsync();
            if (query == null)
            {
                var sameBatchNumberInvoices = await _commandBus.SendAsync(new GetInternalBnInvoiceQuery
                {
                    ProductId = request.ProductId, SupplierId = request.SupplierId,
                    VendorBatchNumber = request.VendorBatchNumber
                });
                internalBatchNumber = request.VendorBatchNumber + "_" + sameBatchNumberInvoices ;
            }
            else internalBatchNumber = query;
            return internalBatchNumber;
        }
    }
}