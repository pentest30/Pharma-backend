using System;
using System.Collections.Generic;
using GHPCommerce.Modules.Sales.Entities;

namespace GHPCommerce.Modules.Sales.DTOs
{
    public class OrderDto
    {
        public string CommandType { get; set; }
        public Guid Id { get; set; }
        public Guid? CustomerId { get; set; }
        public string CustomerName { get; set; }
        public Guid? SupplierId { get; set; }
        public string SupplierName { get; set; }
        public uint OrderStatus { get; set; }
        public decimal OrderTotal { get; set; }
        
        public decimal TotalBrut { get; set; }
        public decimal TotalDiscountHT { get; set; }

        public Decimal OrderDiscount { get; set; }

        public string OrderNumber { get; set; }
        public uint PaymentStatus { get; set; }
        public DateTime? ExpectedShippingDate { get; set; }
        public DateTime? OrderDate { get; set; }

        public string RefDocument { get; set; }
        public uint OrderType { get; set; }
        public DateTimeOffset CreatedDateTime { get; set; }
        public Guid CreatedByUserId { get; set; }
        public Guid? UpdatedByUserId { get; set; }

        public DateTimeOffset? UpdatedDateTime { get; set; }
        public string Code { get; set; }
        public string RefDocumentHpcs { get; set; }
        public DateTime? DateDocumentHpcs { get; set; }

        public List<OrderItem> OrderItems { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public string Type { get; set; }
        public Guid OrderId { get; set; }
        public string Status { get; set; }
        public bool Psychotropic { get; set; }
        public string CodeAx { get; set; }
        public decimal OrderBenefit { get; set; }
        public bool ToBeRespected { get; set; }
        public string DriverName { get; set; }
        public string OrderNumberSequence { get; set; }
        public string ErrorMsg { get; set; }
        public bool  IsSpecialOrder { get; set; }


    }
}
