using System;

namespace GHPCommerce.Core.Shared.Contracts.Orders.Dtos
{
    public class OrderItemDtoV2
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }
        public double Discount { get; set; }
        public double ExtraDiscount { get; set; }

        public double Tax { get; set; }
        public decimal UnitPriceInclTax { get; set; }
        public string VendorBatchNumber { get; set; }
        public string InternalBatchNumber { get; set; }
        public decimal PurchaseUnitPrice { get; set; }
        public decimal PFS { get; set; }
        public decimal PpaHT { get; set; }
        public string ProductCode { get; set; }
    }
}