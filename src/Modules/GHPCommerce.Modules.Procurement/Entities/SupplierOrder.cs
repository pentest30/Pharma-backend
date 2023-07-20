using System;
using System.Collections.Generic;
using GHPCommerce.Domain.Domain.Common;

namespace GHPCommerce.Modules.Procurement.Entities
{
    public class SupplierOrder : AggregateRoot<Guid>, IEntitySequenceNumber
    {
        /// <summary>
        /// gets or sets order date
        /// </summary>
        public DateTime OrderDate { get; set; }

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
        public ProcurmentOrderStatus OrderStatus { get; set; }

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
        public bool Psychotropic { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<SupplierOrderItem> OrderItems { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<SupplierInvoice> Invoices { get; set; }

        public string OrderNumber => "CF-"+OrderDate.Date.ToString("yy-MM-dd").Substring(0,2)
                                          +"/" +"0000000000".Substring(0,10-SequenceNumber.ToString().Length)+ SequenceNumber;

        public int SequenceNumber { get; set; }
        public Guid OrganizationId { get; set; }
    }
}