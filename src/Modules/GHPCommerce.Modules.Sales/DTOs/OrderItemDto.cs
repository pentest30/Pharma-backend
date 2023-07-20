using GHPCommerce.Core.Shared.Contracts.Orders.Common;
using GHPCommerce.Domain.Domain.Common;
using System;

namespace GHPCommerce.Modules.Sales.DTOs
{
    public class OrderItemDto : Entity<Guid>, IOrderItemBase
    {
        public Guid? OrderId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public double Discount { get; set; }
        public double Tax { get; set; }
        public decimal UnitPriceInclTax { get; set; }
        public string VendorBatchNumber { get; set; }
        public string InternalBatchNumber { get; set; }
        public string Color { get; set; }
        public string Size { get; set; }
        public decimal PFS { get; set; }
        public decimal PpaHT { get; set; }
        public decimal PpaTTC { get; set; }
        public decimal PpaPFS { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public decimal TotalInlTax { get; set; }
        public decimal TotalExlTax { get; set; }
        public double ExtraDiscount { get; set; }
        public bool AcceptedOnAx { get; set; }
        public Guid? ZoneGroupId { get; set; }

    }
}
