using System;
using GHPCommerce.Domain.Domain.Common;

namespace GHPCommerce.Modules.PreparationOrder.Entities
{
    public class DeleiveryOrderItem : Entity<Guid>
    {
        public DeleiveryOrderItem()
        {
            CreatedDateTime = DateTimeOffset.Now;
        }

        public Guid? DeleiveryOrderId { get; set; }      
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal PurchaseUnitPrice { get; set; }
        public double Discount { get; set; }
        public double ExtraDiscount { get; set; }
        public double Tax { get; set; }
        public string VendorBatchNumber { get; set; }
        public string InternalBatchNumber { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public decimal PFS { get; set; }
        public decimal PpaHT { get; set; }
        public int Packing { get; set; }

        public  DeleiveryOrder DeleiveryOrder { get; set; }

    }
}