using System;
using System.ComponentModel.DataAnnotations;
using FluentValidation;
using GHPCommerce.Domain.Domain.Commands;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace GHPCommerce.Core.Shared.Contracts.Batches.Commands
{
    public class CreateBatchCommand : ICommand<Tuple<Guid, ValidationResult>>
    {
        public Guid OrganizationId { get; set; }
        public Guid ProductId { get; set; }

        [MaxLength(100)]
        public string OrganizationName { get; set; }

        [MaxLength(100)]
        public string ProductCode { get; set; }

        [MaxLength(100)]
        public string ProductFullName { get; set; }
        [MaxLength(100)]
        public string VendorBatchNumber { get; set; }

        [MaxLength(100)]
        public string InternalBatchNumber { get; set; }
        public double? PurchaseUnitPrice { get; set; }
        public float? PurchaseDiscountRatio { get; set; }
        public double? SalesUnitPrice { get; set; }
        public float? SalesDiscountRatio { get; set; }
        public int Packing { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public decimal PFS { get; set; }
        public decimal PpaHT { get; set; }
        public decimal PpaTTC { get; set; }
        public decimal PpaPFS { get; set; }
        public Guid OrderId { get; set; }
        public string RefDoc { get; set; }
        public int Quantity { get; set; }
        public Guid SupplierId { get; set; }
        public string SupplierName { get; set; }
    }

    public class CreateBatchCommandValidator : AbstractValidator<CreateBatchCommand>
    {
        public CreateBatchCommandValidator()
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
          
            RuleFor(v => v.Quantity)
                .GreaterThan(0);
          
            
        }
    }
}