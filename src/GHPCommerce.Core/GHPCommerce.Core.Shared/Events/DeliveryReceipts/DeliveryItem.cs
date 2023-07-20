using System;

namespace GHPCommerce.Core.Shared.Events.DeliveryReceipts
{
    public  class DeliveryItem
    {
        public string InternalBatchNumber { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int Packing { get; set; }
        public string VendorBatchNumber { get; set; }

        public decimal PFS { get; set; }
        public decimal Ppa { get; set; }
        public int Quantity { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }   
    }
}