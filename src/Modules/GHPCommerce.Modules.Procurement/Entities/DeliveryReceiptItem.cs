using System;
using GHPCommerce.Domain.Domain.Common;

namespace GHPCommerce.Modules.Procurement.Entities
{
    public class DeliveryReceiptItem : Entity<Guid>
    {
        public Guid? DeliveryReceiptId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal SalePrice { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DeliveryReceipt DeliveryReceipt { get; set; }
        public int Packing { get; set; }
        public int PackingNumber { get; set; }
        public decimal PFS { get; set; }
        public decimal ppa { get; set; }
        /// <summary>
        /// vrac
        /// </summary>
        public int Bulk { get; set; }
        public string VendorBatchNumber { get; set; }
        public string InternalBatchNumber { get; set; }

    }
}