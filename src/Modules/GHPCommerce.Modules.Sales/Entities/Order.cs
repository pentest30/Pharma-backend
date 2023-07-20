using System;
using System.Collections.Generic;
using GHPCommerce.Core.Shared.Enums;
using GHPCommerce.Domain.Domain.Common;
using GHPCommerce.Domain.Domain.Identity;

namespace GHPCommerce.Modules.Sales.Entities
{
    public class Order : AggregateRoot<Guid>
    {
        public Order()
        {
            OrderItems = new List<OrderItem>();
            OrderStatus = OrderStatus.Pending;
            PaymentStatus = PaymentStatus.Pending;
        }
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
        public OrderStatus OrderStatus { get; set; }
        /// <summary>
        /// gets or sets discount order
        /// </summary>
        public decimal OrderDiscount { get; set; }
        /// <summary>
        /// gets or sets total
        /// </summary>
        public decimal OrderTotal { get; set; }
        public decimal OrderBenefit { get; set; }
        public decimal OrderBenefitRate { get; set; }

        /// <summary>
        /// gets or sets order number
        /// </summary>
        public string OrderNumber => "BC-"+OrderDate.Date.ToString("yy-MM-dd").Substring(0,2)
                                     +"-" +"0000000000".Substring(0,10-OrderNumberSequence.ToString().Length)+ OrderNumberSequence;
        /// <summary>
        /// gets or sets order number sequence
        /// </summary>
        public int OrderNumberSequence { get; set; }
        /// <summary>
        /// gets or sets payment Status
        /// </summary>
        public PaymentStatus PaymentStatus { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? ExpectedShippingDate { get; set; }
        /// <summary>
        /// gets or sets document's reference
        /// </summary>
        public string RefDocument { get; set; }
        public CancellationReason CancellationReason { get; set; }
        public Guid? CanceledBy { get; set; }
        public DateTime? CanceledTime { get; set; }
        public string RejectedReason { get; set; }
        public Guid? RejectedBy { get; set; }
        public DateTime? RejectedTime { get; set; }
        public Guid? ApprovedBy { get; set; }
        public DateTime? ApprovedTime { get; set; }
        public Guid? ProcessedBy { get; set; }
        public DateTime? ProcessingTime { get; set; }
        public Guid? ShippedBy { get; set; }
        public DateTime? ShippingTime { get; set; }
        public Guid? CompletedBy { get; set; }
        public DateTime? CompletedTime { get; set; }
        public Guid? PaidBy { get; set; }
        public DateTime? PaidDateTime { get; set; }
        public Guid? DefaultSalesPersonId { get; set; }
        public OrderType OrderType { get; set; }
        public string CreatedBy{ get; set; }
        public string UpdatedBy { get; set; }
        /// <summary>
        /// code ax 2009
        /// </summary>
        public string CodeAx { get; set; }

        public bool ToBeRespected { get; set; }
        public bool IsSpecialOrder { get; set; }

        public bool QuantitiesReleased { get; set; } 
        public string RefDocumentHpcs { get; set; }
        public DateTime? DateDocumentHpcs { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// 
        //public DateTimeOffset? CreatedDateTime { get; set; }

        public List<OrderItem> OrderItems { get; set; }

        public Guid? GuestId { get; set; }
        public string DriverName { get; set; }
        public string Comment { get; set; }
         
        public void SetOrderInfo(Guid createdBy, string customerName, Guid customerId, string supplierName, Guid? defaultSalesPerson)
        { 
            CustomerName = customerName;
            CustomerId = customerId;
            SupplierName = supplierName;
            DefaultSalesPersonId = defaultSalesPerson;

        }
    }
}
