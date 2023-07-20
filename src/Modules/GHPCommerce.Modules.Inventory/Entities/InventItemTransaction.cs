using GHPCommerce.Domain.Domain.Common;
using System;

namespace GHPCommerce.Modules.Inventory.Entities
{
    public class InventItemTransaction : AggregateRoot<Guid>
    {
        public Guid OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public string ProductCode { get; set; }
        public string ProductFullName { get; set; }
        public Guid CustomerId { get; set; }
        public Guid SupplierId { get; set; }
        public Guid InventId { get; set; }
        public string CustomerName { get; set; }
        public string SupplierName { get; set; }

        public double Quantity { get; set; }
        public double OriginQuantity { get; set; }
        public double NewQuantity { get; set; }

        public Guid? OrderId { get; set; }
        public string OrderNumberSequence { get; set; }
        public DateTime OrderDate { get; set; }
        public string RefDoc { get; set; }
        public Guid? BlId { get; set; }
        public DateTime TransactionTime { get; set; }
        public Guid TransactionTypeId { get; set; }
        public int TransactionCode { get; set; }
        
        public Invent Invent { get; set; }
        public string VendorBatchNumber { get; set; }
        public string InternalBatchNumber { get; set; }
        public TransactionType TransactionType { get; set; }
        public bool StockEntry { get; set; }
        public Guid ProductId { get; set; }
    }

    public enum TransactionType
    {
        SupplierReception = 10,
        SupplierInvoice = 20,
        CustomerReturn = 30,
        Readjustment   = 40,
        InterUnitTransfer = 50,
        DeliveryNote = 60,
        CustomerInvoice =70,
        Incineration = 80,
        Transfer = 90,
        ManualTransfer = 100
        
    }
}
