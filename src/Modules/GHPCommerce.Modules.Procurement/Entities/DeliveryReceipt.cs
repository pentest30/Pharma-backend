using System;
using System.Collections.Generic;
using GHPCommerce.Domain.Domain.Common;

namespace GHPCommerce.Modules.Procurement.Entities
{
    public class DeliveryReceipt  :AggregateRoot<Guid>, IEntitySequenceNumber
    {
        public string DocRef { get; set; }
        public string DeliveryReceiptNumber { get; set; }
        public string InvoiceNumber { get; set; }
        public Guid InvoiceId { get; set; }
        public SupplierInvoice Invoice { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime DeliveryReceiptDate { get; set; }
        /// <summary>
        /// Montant total TTC
        /// </summary>
        public decimal TotalAmount { get; set; }
        /// <summary>
        /// Montant total TVA
        /// </summary>
        public decimal TaxTotalAmount { get; set; }
        /// <summary>
        /// Montant total HT de la réception 
        /// </summary>
        public decimal ReceiptsAmountExcTax { get; set; }
        /// <summary>
        /// Montant total des remises
        /// </summary>
        public decimal DiscountTotalAmount { get; set; }
        public InvoiceStatus Status { get; set; }
        public List<DeliveryReceiptItem> Items { get; set; }
        public Guid OrganizationId { get; set; }

        public int SequenceNumber { get; set; }
    }
}