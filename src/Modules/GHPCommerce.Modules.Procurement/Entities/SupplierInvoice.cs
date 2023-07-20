using System;
using System.Collections.Generic;
using GHPCommerce.Domain.Domain.Common;

namespace GHPCommerce.Modules.Procurement.Entities
{
   
    public class SupplierInvoice : AggregateRoot<Guid>, IEntitySequenceNumber
    {
        public SupplierInvoice()
        {
            RefDocument = "";
            InvoiceNumber = "";
        }
        /// <summary>
        /// 
        /// </summary>
        public DateTime InvoiceDate { get; set; }

        /// <summary>
        /// gets or sets customer id
        /// </summary>
        public Guid CustomerId { get; set; }

        /// <summary>
        /// gets or sets customer's name
        /// </summary>
        public string CustomerName { get; set; }

        /// <summary>
        /// gets or sets supplier id
        /// </summary>
        public Guid SupplierId { get; set; }

        /// <summary>
        /// gets or sets supplier name
        /// </summary>
        public string SupplierName { get; set; }

        /// <summary>
        /// gets or sets order status
        /// </summary>
        public InvoiceStatus InvoiceStatus { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime? ExpectedDeliveryDate { get; set; }

        /// <summary>
        /// gets or sets document's reference
        /// </summary>
        public string RefDocument { get; set; }
        /// <summary>
        /// 
        /// </summary>

        public string InvoiceNumber { get; set; }
        /// <summary>
        /// 
        /// </summary>

        public SupplierOrder Order { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Guid OrderId { get; set; }
        /// <summary>
        /// motant total
        /// </summary>
        public decimal TotalAmount { get; set; }
        public decimal TotalAmountExlTax { get; set; }
        /// <summary>
        /// Montant des réceptions
        /// </summary>

        public decimal ReceiptsAmount { get; set; }

        public decimal RemainingAmount => TotalAmount - ReceiptsAmount;
        public List<SupplierInvoiceItem> Items { get; set; }
        public List<DeliveryReceipt> Receipts { get; set; }
        public string InvoiceSequenceNumber => "FF-"+ InvoiceDate.Date.ToString("yy-MM-dd").Substring(0,2)
                                           +"/" +"0000000000".Substring(0,10-SequenceNumber.ToString().Length)+ SequenceNumber;

        public int SequenceNumber { get; set; }
        public Guid OrganizationId { get; set; }
    }
}