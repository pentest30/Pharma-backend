using System;
using System.Collections.Generic;

namespace GHPCommerce.Modules.Sales.DTOs
{
    public class OrderDtoV2
    {
        public Guid Id { get; set; }
        public Guid? CustomerId { get; set; }
        public string CustomerName { get; set; }
        public Guid? SupplierId { get; set; }
        public string SupplierName { get; set; }
        public uint OrderStatus { get; set; }
        public decimal OrderDiscount { get; set; }
        public decimal OrderTotal { get; set; }
        public string OrderNumber { get; set; }
        public uint PaymentStatus { get; set; }
        public DateTime? ExpectedShippingDate { get; set; }
        public string RefDocument { get; set; }
        public DateTimeOffset CreatedDateTime { get; set; }
        public string CancellationReason { get; set; }
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
        public uint OrderType { get; set; }
        public string CodeAx { get; set; }
        public List<OrderItemDto> OrderItems { get; set; }
        public string OrderNumberSequence { get; set; }
        public bool ToBeRespected { get; set; }
        public bool IsSpecialOrder { get; set; }

        public string DriverName { get; set; }
        public decimal TotalBrut { get; set; }
        public decimal TotalDiscountHT { get; set; }
    }
}
