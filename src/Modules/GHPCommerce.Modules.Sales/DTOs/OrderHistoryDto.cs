using System;

namespace GHPCommerce.Modules.Sales.DTOs
{
    public class OrderHistoryDto
    {
        public string ClientCode { get; set; }
        public string ClientName { get; set; }
        public string ProductName { get; set; }
        public DateTime? ShippingDate { get; set; }
        public string CommandStatus { get; set; }
        public string ProductCode { get; set; }
        public int Quantity { get; set; }
        public string OrderNumber { get; set; }

    }
}
