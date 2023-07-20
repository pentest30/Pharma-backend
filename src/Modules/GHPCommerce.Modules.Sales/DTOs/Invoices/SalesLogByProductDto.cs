using System;

namespace GHPCommerce.Modules.Sales.DTOs.Invoices
{
    public class SalesLogByProductDto
    {
      
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public int Quantity { get; set; }
        public string Manufacturer { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalTTC { get; set; }
        public decimal TotalHT { get; set; }
    }
}