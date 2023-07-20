using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using GHPCommerce.CrossCuttingConcerns.Exceptions;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.Procurement.Entities;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace GHPCommerce.Modules.Procurement.Commands.Invoices
{
    public class UpdateInvoiceItemCommand : ICommand<ValidationResult>
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
    public class UpdateInvoiceItemCommandValidator : AbstractValidator<UpdateInvoiceItemCommand>
    {
        public UpdateInvoiceItemCommandValidator()
        {
            RuleFor(v => v.ProductCode)
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

        public class UpdateInvoiceItemCommandHandler : ICommandHandler<UpdateInvoiceItemCommand, ValidationResult>
        {
            private readonly IRepository<SupplierInvoice, Guid> _repository;
            private readonly ICurrentOrganization _currentOrganization;
            private readonly IMapper _mapper;
            private readonly Logger _logger;

            public UpdateInvoiceItemCommandHandler(IRepository<SupplierInvoice, Guid> repository, 
                ICurrentOrganization currentOrganization,
                IMapper mapper,  Logger logger)
            {
                _repository = repository;
                _currentOrganization = currentOrganization;
                _mapper = mapper;
                _logger = logger;
            }
            public  async Task<ValidationResult> Handle(UpdateInvoiceItemCommand request, CancellationToken cancellationToken)
            {
                var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
                if (orgId == default) throw new InvalidOperationException("");
                ValidationResult? validations = default;
                var validator = new UpdateInvoiceItemCommandValidator();
                var validationErrors = await validator.ValidateAsync(request, cancellationToken);
                if (!validationErrors.IsValid)
                    return validationErrors;

                try
                {
                    var invoice = await _repository.Table
                        .Include(x=>x.Items)
                        .FirstOrDefaultAsync(x => x.Id == request.InvoiceId, cancellationToken: cancellationToken);
                    if (invoice == null) throw new NotFoundException($"invoice with id {request.InvoiceId} was not found");
                    var line = invoice.Items.FindIndex(x =>
                        x.ProductId == request.ProductId && x.InternalBatchNumber == request.InternalBatchNumber);
                    if (line == -1) throw new NotFoundException($"line  was not found");
                    invoice.TotalAmount = request.TotalAmount;
                    invoice.TotalAmountExlTax = request.TotalAmountExlTax;
                    invoice.InvoiceDate = request.InvoiceDate;
                    invoice.RefDocument = request.DocumentRef;
                    invoice.Items[line] = _mapper.Map<SupplierInvoiceItem>(request);
                    _repository.Update(invoice);
                    await _repository.UnitOfWork.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    validations = new ValidationResult
                        {Errors = {new ValidationFailure("Transaction rolled back", ex.Message)}};
                    _logger.Error(ex.Message);
                    _logger.Error(ex.InnerException?.Message);
                }

                return validations;
            }
        }
    }
}