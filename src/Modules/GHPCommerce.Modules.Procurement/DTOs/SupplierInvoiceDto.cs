using System;
using System.Collections.Generic;
using GHPCommerce.Modules.Procurement.Entities;

namespace GHPCommerce.Modules.Procurement.DTOs
{
    public class SupplierInvoiceDto
    {
        public Guid Id { get; set; }
        public DateTime InvoiceDate { get; set; }
        public Guid InvoiceId { get; set; }

        public Guid CustomerId { get; set; }

        public string CustomerName { get; set; }

        public Guid SupplierId { get; set; }

        public string SupplierName { get; set; }

        public InvoiceStatus InvoiceStatus { get; set; }

        public DateTime? ExpectedDeliveryDate { get; set; }

        public string RefDocument { get; set; }

        public string InvoiceSequenceNumber { get; set; }
        public string InvoiceNumber { get; set; }

        public SupplierOrder Order { get; set; }

        public Guid OrderId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalAmountExlTax { get; set; }
        public decimal ReceiptsAmount { get; set; }

        public decimal RemainingAmount => TotalAmount - ReceiptsAmount;
        public string Status { get; set; }
        public Guid CreatedByUserId { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public DateTimeOffset CreatedDateTime { get; set; }
        public DateTimeOffset? UpdatedDateTime { get; set; }

        public Guid UpdatedByUserId { get; set; }
        public List<SupplierInvoiceItemDto> Items { get; set; }

    }
}