using System;
using System.Collections.Generic;
using GHPCommerce.Modules.Procurement.Entities;

namespace GHPCommerce.Modules.Procurement.DTOs
{
    public class DeliveryReceiptDto 
    {
        public Guid Id { get; set; }
        public string DocRef { get; set; }
        public string DeliveryReceiptSequenceNumber { get; set; }
        public Guid? DeliveryReceiptId { get; set; }
        public string InvoiceNumber { get; set; }
        public Guid InvoiceId { get; set; }
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
        public string DeliveryReceiptStatus { get; set; }

        public Guid CreatedByUserId { get; set; }

        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset CreatedDateTime { get; set; }
        public DateTimeOffset? UpdatedDateTime { get; set; }

        public Guid UpdatedByUserId { get; set; }
        public List<DeliveryReceiptItemDto> Items { get; set; }
        public Guid OrganizationId { get; set; }
    }
}