using System;
using System.Collections.Generic;
using GHPCommerce.Modules.Sales.Entities;

namespace GHPCommerce.Modules.Sales.Models
{
    public class OrderModel
    {
        public Guid Id { get; set; }
        public Guid? CustomerId { get; set; }
        public string CustomerName { get; set; }
        public Guid? SupplierId { get; set; }
        public string SupplierName { get; set; }
        public short OrderStatus { get; set; }
        public decimal OrderDiscount { get; set; }
        public decimal OrderTotal { get; set; }
        public string OrderNumber { get; set; }
        public short PaymentStatus { get; set; }
        public DateTime? ExpectedShippingDate { get; set; }
        public string RefDocument { get; set; }
        
        public List<OrderItem> OrderItems { get; set; }
    } 
}
