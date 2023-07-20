using System;

namespace GHPCommerce.Modules.Sales.DTOs
{
    public class DiscountDto
    {
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; }
        public Guid ProductId { get; set; }
        public int ThresholdQuantity { get; set; }
        public decimal DiscountRate { get; set; }
        public DateTime from { get; set; }
        public DateTime to { get; set; }
        public string fromDateShort { get; set; }
        public string toDateShort { get; set; }

        public string ProductFullName { get; set; }

    }
}
