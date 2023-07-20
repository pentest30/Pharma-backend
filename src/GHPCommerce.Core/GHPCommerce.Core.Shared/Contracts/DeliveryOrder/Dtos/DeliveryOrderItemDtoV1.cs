using System;

namespace GHPCommerce.Core.Shared.Contracts.DeliveryOrder.Dtos
{
    public class DeliveryOrderItemDtoV1
    {
        public string InternalBatchNumber { get; set; }
        public int Packaging { get; set; }
        public decimal PFS { get; set; }
        public decimal Ppa { get; set; }
        public int Quantity { get; set; }
        public decimal PurchaseUnitPrice { get; set; }
        public decimal UnitPrice { get; set; }

        public double Tax { get; set; }
        public decimal PpaHT { get; set; }

        public Guid ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string VendorBatchNumber { get; set; }
        public double Discount { get; set; }
        public double ExtraDiscount { get; set; }
        public DateTime? ExpiryDate { get; set; }

    }
}