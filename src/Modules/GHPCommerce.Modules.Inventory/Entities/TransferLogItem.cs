using System;
using GHPCommerce.Domain.Domain.Common;

namespace GHPCommerce.Modules.Inventory.Entities
{
    public class TransferLogItem: Entity<Guid>
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public string InternalBatchNumber { get; set; }
        public Guid InventId { get; set; }
        public Invent Invent { get; set; }
        public double Quantity { get; set; }
        public DateTime ExpiryDate { get; set; }
        public Guid TransferLogId { get; set; }
        public TransferLog TransferLog { get; set; }
    }
}