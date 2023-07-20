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

namespace GHPCommerce.Modules.Procurement.Commands.Receipts
{
    public class UpdateReceiptItemCommand : CreateReceiptItemCommand
    {
    }
    public class UpdateReceiptItemCommandValidator : AbstractValidator<CreateReceiptItemCommand>
    {
        public UpdateReceiptItemCommandValidator()
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
    public  class UpdateReceiptItemCommandHandler : ICommandHandler<UpdateReceiptItemCommand, ValidationResult>
    {
        private readonly IRepository<DeliveryReceipt, Guid> _repository;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly IMapper _mapper;
        private readonly Logger _logger;

        public UpdateReceiptItemCommandHandler(IRepository<DeliveryReceipt, Guid> repository, 
            ICurrentOrganization currentOrganization,
            IMapper mapper,  Logger logger)
        {
            _repository = repository;
            _currentOrganization = currentOrganization;
            _mapper = mapper;
            _logger = logger;
        }
        public  async Task<ValidationResult> Handle(UpdateReceiptItemCommand request, CancellationToken cancellationToken)
        {
            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (orgId == default) throw new InvalidOperationException("");
            ValidationResult? validations = default;
            var validator = new UpdateReceiptItemCommandValidator();
            var validationErrors = await validator.ValidateAsync(request, cancellationToken);
            if (!validationErrors.IsValid)
                return validationErrors;

            try
            {
                var deliveryReceipt = await _repository.Table.Include(x=>x.Items)
                    .FirstOrDefaultAsync(x => x.Id == request.DeliveryReceiptId, cancellationToken: cancellationToken);
                if (deliveryReceipt == null) throw new NotFoundException($"invoice with id {request.InvoiceId} was not found");
                var line = deliveryReceipt.Items.FindIndex(x => x.ProductId == request.ProductId 
                                                                && x.InternalBatchNumber == request.InternalBatchNumber);
                if (line == -1) throw new NotFoundException($"line  was not found");
                deliveryReceipt.DocRef = request.DocRef;
                deliveryReceipt.InvoiceDate = request.InvoiceDate;
                deliveryReceipt.InvoiceNumber = request.InvoiceNumber;
                deliveryReceipt.TotalAmount = request.TotalAmount;
                deliveryReceipt.TaxTotalAmount = request.TaxTotalAmount;
                deliveryReceipt.ReceiptsAmountExcTax = request.ReceiptsAmountExcTax;
                deliveryReceipt.Items[line] = _mapper.Map<DeliveryReceiptItem>(request);
                _repository.Update(deliveryReceipt);
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