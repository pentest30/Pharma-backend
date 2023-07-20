using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using GHPCommerce.Core.Shared.Services;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Infra.OS;
using GHPCommerce.Modules.Procurement.Entities;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Modules.Procurement.Commands.Receipts
{
    public class CreateReceiptItemCommand : ICommand<ValidationResult>
    {
        public Guid DeliveryReceiptId { get; set; }
        public Guid OrganizationId { get; set; }
        public string DocRef { get; set; }
        public string DeliveryReceiptNumber { get; set; }
        public string InvoiceNumber { get; set; }
        public Guid InvoiceId { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime DeliveryReceiptDate { get; set; }
        /// <summary>
        /// Montant total TTC
        /// </summary>
        public decimal TotalAmount { get; set; }
        /// <summary>
        /// Montant total TVA
        /// </summary>
        public decimal TaxTotalAmount { get; set; }
        /// <summary>
        /// Montant total HT de la réception 
        /// </summary>
        public decimal ReceiptsAmountExcTax { get; set; }
        /// <summary>
        /// Montant total des remises
        /// </summary>
        public decimal DiscountTotalAmount { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal SalePrice { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int Packing { get; set; }
        public int PackingNumber { get; set; }
        public decimal PFS { get; set; }
        public decimal Ppa { get; set; }
        /// <summary>
        /// vrac
        /// </summary>
        public int Bulk { get; set; }
        public string VendorBatchNumber { get; set; }
        public string InternalBatchNumber { get; set; }
    }
    public class CreateReceiptItemCommandValidator : AbstractValidator<CreateReceiptItemCommand>
    {
        public CreateReceiptItemCommandValidator()
        {
            RuleFor(v => v.ProductCode)
                .MaximumLength(200).
                NotEmpty();
            RuleFor(v => v.InvoiceNumber)
                .MaximumLength(200).
                NotEmpty();
          
            RuleFor(v => v.ProductId)
                .Must(x => x != Guid.Empty);
         
          
            RuleFor(v => v.InvoiceId)
                .Must(x => x != Guid.Empty);
            RuleFor(v => v.InvoiceDate)
                .Must(x => x != default);
            
            RuleFor(v => v.Quantity)
                .GreaterThan(0);
            RuleFor(v => v.DocRef)
                .NotEmpty();
        }
    }
    public  class CreateReceiptItemCommandHandler : ICommandHandler<CreateReceiptItemCommand, ValidationResult>
    {
        private readonly IRepository<DeliveryReceipt, Guid> _repository;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly IMapper _mapper;
        private readonly ISequenceNumberService<DeliveryReceipt, Guid> _sequenceNumberService;

        public CreateReceiptItemCommandHandler(IRepository<DeliveryReceipt, Guid> repository, 
            ICurrentOrganization currentOrganization,
            IMapper mapper,
            ISequenceNumberService<DeliveryReceipt, Guid> sequenceNumberService)
        {
            _repository = repository;
            _currentOrganization = currentOrganization;
            _mapper = mapper;
            _sequenceNumberService = sequenceNumberService;
        }
        public async Task<ValidationResult> Handle(CreateReceiptItemCommand request, CancellationToken cancellationToken)
        {
            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (orgId == default) throw new InvalidOperationException("Resources not allowed");

            ValidationResult validations = default;
            var validator = new CreateReceiptItemCommandValidator();
            var validationErrors = await validator.ValidateAsync(request, cancellationToken);
            if (!validationErrors.IsValid)
                return validationErrors;

            bool newInvoice = false;
            var keysq = nameof(DeliveryReceipt) + orgId;
            var deliveryReceipt = await _repository.Table.Include(x=>x.Items).FirstOrDefaultAsync(x => x.Id == request.DeliveryReceiptId, cancellationToken: cancellationToken);
            if (deliveryReceipt == null)
            {
                deliveryReceipt = _mapper.Map<DeliveryReceipt>(request);
                deliveryReceipt.OrganizationId = orgId.Value;
                deliveryReceipt.Status = InvoiceStatus.Created;
                
                await LockProvider<string>.WaitAsync(keysq, cancellationToken);
                var sq = await _sequenceNumberService.GenerateSequenceNumberAsync(DateTime.Now, orgId.Value);
                deliveryReceipt.SequenceNumber = sq;
                
                newInvoice = true;
                
            }
            else newInvoice = false;
            deliveryReceipt.TotalAmount = request.TotalAmount;
            deliveryReceipt.TaxTotalAmount = request.TaxTotalAmount;
            deliveryReceipt.ReceiptsAmountExcTax = request.ReceiptsAmountExcTax;
            var item = _mapper.Map<DeliveryReceiptItem>(request);
            if (deliveryReceipt.Items == null) deliveryReceipt.Items = new List<DeliveryReceiptItem>();
            deliveryReceipt.Items.Add(item);
            if (newInvoice)
            {
                _repository.Add(deliveryReceipt);
                LockProvider<string>.Release(keysq);
            }
            else _repository.Update(deliveryReceipt);
            await _repository.UnitOfWork.SaveChangesAsync();
            return default!;
        }
    }
}