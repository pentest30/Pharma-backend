using System;
using GHPCommerce.Modules.Inventory.Entities;

namespace GHPCommerce.Modules.Inventory.DTOs
{
    public class InventItemTransactionDto
    {
        public string OrganizationName { get; set; }
        public string ProductCode { get; set; }
        public string ProductFullName { get; set; }
        public double Quantity { get; set; }
        public double OriginQuantity { get; set; }
        public double NewQuantity { get; set; }
        public Guid? OrderId { get; set; }
        public string OrderNumberSequence { get; set; }
        public string CustomerName { get; set; }
        public DateTime OrderDate { get; set; }
        public string VendorBatchNumber { get; set; }
        public string InternalBatchNumber { get; set; }
        public TransactionType TransactionType { get; set; }
        public bool StockEntry { get; set; }
        public string RefDoc { get; set; }
        public Guid InventId { get; set; }
        public string ZoneName { get; set; }
        public string StockStateName { get; set; }
        public Guid? StockStateId { get; set; }
        public Guid? ZoneId { get; set; }
        public string SupplierName { get; set; }

    }
}
