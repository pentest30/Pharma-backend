using System;

namespace GHPCommerce.Modules.Sales.DTOs
{
    public class OrderByDateDto
    {
        public string CommandType { get; set; }
        public Guid Id { get; set; }
        public Guid? CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string OrderStatus { get; set; }
        public decimal OrderTotal { get; set; }
        public decimal TotalNet { get; set; }
        public decimal TotalIncludeDiscount { get; set; }
        public string OrderNumber { get; set; }
        public string CreatedBy { get; set; }
    }
}