using System;
using System.Collections.Generic;

namespace GHPCommerce.Core.Shared.Contracts.Orders.Dtos
{
    public class OrderDtoV3
    {
        public string CommandType { get; set; }
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
        public DateTime? OrderDate { get; set; }

        public string RefDocument { get; set; }
        public uint OrderType { get; set; }
        public DateTimeOffset CreatedDateTime { get; set; }
        public Guid CreatedByUserId { get; set; }
        public Guid? UpdatedByUserId { get; set; }
        public int OrderNumberSequence { get; set; }

        public DateTimeOffset? UpdatedDateTime { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public string Type { get; set; }
        public Guid OrderId { get; set; }
        public string Status { get; set; }
        public bool Psychotropic { get; set; }
        public List<OrderItemDtoV2> OrderItems { get; set; }
        public string CodeAx { get; set; }
        public bool ToBeRespected { get; set; }
    }
}
