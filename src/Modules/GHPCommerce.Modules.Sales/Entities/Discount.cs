using GHPCommerce.Domain.Domain.Common;
using System;

namespace GHPCommerce.Modules.Sales.Entities
{
    public class Discount : AggregateRoot<Guid>
    {
        public Guid OrganizationId { get; set; }
        public Guid ProductId { get; set; }
        public int  ThresholdQuantity { get; set; }
        public decimal DiscountRate{ get; set; }
        public DateTime from { get; set; }
        public DateTime to { get; set; }
        public string ProductFullName { get; set; }

    }

}
