using System;
using FluentValidation;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Core.Shared.Contracts.Transactions
{
    public class CreateAtSupplierInventTransactionCommand :  ICommand<ValidationResult>
    {
        public Guid OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public string ProductCode { get; set; }
        public string ProductFullName { get; set; }
        public Guid CustomerId { get; set; }
        public Guid SupplierId { get; set; }
        public Guid InventId { get; set; }
        public string CustomerName { get; set; }
        public string SupplierName { get; set; }

        public double Quantity { get; set; }
        public double OriginQuantity { get; set; }
        public double NewQuantity { get; set; }

        public Guid? OrderId { get; set; }
        public string OrderNumberSequence { get; set; }
        public DateTime OrderDate { get; set; }
        public string RefDoc { get; set; }
        public Guid? BlId { get; set; }
        public DateTime TransactionTime { get; set; }
        public Guid TransactionTypeId { get; set; }
        public int TransactionCode { get; set; }
        public string VendorBatchNumber { get; set; }
        public string InternalBatchNumber { get; set; }
        public bool StockEntry { get; set; }
        public Guid ProductId { get; set; }
    }

    public class CreateAtSupplierInventTransactionCommandValidator : AbstractValidator<CreateAtSupplierInventTransactionCommand>
    {
        public CreateAtSupplierInventTransactionCommandValidator()
        {
            RuleFor(v => v.ProductCode)
                .MaximumLength(200).
                NotEmpty();
            RuleFor(v => v.ProductFullName)
                . NotEmpty();
            RuleFor(v => v.ProductId)
                .Must(x => x != Guid.Empty);
            RuleFor(v => v.OrderId)
                .Must(x => x != Guid.Empty);
            RuleFor(v => v.CustomerId)
                .Must(x => x != Guid.Empty);
            RuleFor(v => v.SupplierId)
                .Must(x => x != Guid.Empty);
            RuleFor(v => v.Quantity)
                .GreaterThan(0);
          
            RuleFor(v => v.RefDoc)
                .NotEmpty();
        }
    }
}