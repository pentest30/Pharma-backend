using System;

namespace GHPCommerce.Modules.Sales.Models
{
    public class DiscountModel
    {
        public Guid OrganizationId { get; set; }
        public Guid ProductId { get; set; }
        public int ThresholdQuantity { get; set; }
        public int DiscountRate { get; set; }
        public DateTime from { get; set; }
        public DateTime to { get; set; }
        
    }
}
