using System;
using FluentValidation;
using FluentValidation.Results;
using GHPCommerce.Core.Shared.Enums;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Core.Shared.Contracts.Orders.Commands
{
    public class OrderItemCreateCommand : ICommand<ValidationResult>, IOrderItem
    { 
        public Guid OrderId { get; set; }
        public Guid ProductId { get; set; }
        public Guid? SalesPersonId { get; set; }
        public string RefDocumentHpcs { get; set; }
        public DateTime? DateDocumentHpcs { get; set; }
        public string Color { get; set; }
        public string Size { get; set; }
        public int Quantity { get; set; }
        public int OldQuantity { get; set; }
        public Guid SupplierOrganizationId { get; set; }
        public DateTime? MinExpiryDate { get; set; }
        public string InternalBatchNumber { get; set; }
        public Guid CustomerId { get; set; }
        public string ProductCode { get; set; }
        public string PackagingCode { get; set; }
        public decimal PFS { get; set; }
        public decimal PpaHT { get; set; }
        public decimal PpaTTC { get; set; }
        public decimal PpaPFS { get; set; }
        public double Tax { get; set; }

        public OrderType OrderType { get; set; }
        public decimal ExtraDiscount { get; set; }
        public decimal Discount { get; set; }
        public string DocumentRef { get; set; }
        public Guid? PickingZoneId { get; set; }
        public string PickingZoneName { get; set; }
        public Guid? ZoneGroupId { get; set; }
        public string ZoneGroupName { get; set; }
        public int Packing { get; set; }
        public bool Thermolabile { get; set; }
        public bool ToBeRespected { get; set; }
        public string DefaultLocation { get; set; }
        public int PickingZoneOrder { get; set; }

    }

    public class OrderItemCreateCommandValidator : AbstractValidator<OrderItemCreateCommand>
    {
        public OrderItemCreateCommandValidator()
        {
            RuleFor(v => v.ProductCode)
                .MaximumLength(200).
                NotEmpty();
            RuleFor(v => v.ProductId)
                .Must(x=>x!=Guid.Empty);
            RuleFor(v => v.CustomerId)
                .Must(x => x != Guid.Empty);
            RuleFor(v => v.OrderId)
                .Must(x => x != Guid.Empty);
            RuleFor(v => v.Quantity)
                .GreaterThan(0);
            // RuleFor(v => v.DocumentRef)
            //     .NotEmpty().When(x=>x.OrderType == OrderType.Psychotrope);
        }
    }
}