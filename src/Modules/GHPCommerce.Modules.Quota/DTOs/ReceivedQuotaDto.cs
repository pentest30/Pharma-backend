using System;

namespace GHPCommerce.Modules.Quota.DTOs
{
    public class ReceivedQuotaDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public int Quantity { get; set; }
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerCode { get; set; }
        public string DateShort { get; set; }
        public string SalesPersonName { get; set; }
        public Guid? SalesPersonId { get; set; }
        public string Status { get; set; }
    }
}