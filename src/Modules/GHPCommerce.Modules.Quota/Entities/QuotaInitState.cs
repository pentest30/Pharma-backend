using System;
using GHPCommerce.Domain.Domain.Common;

namespace GHPCommerce.Modules.Quota.Entities
{
    public class QuotaInitState : AggregateRoot<Guid>
    {
        public Guid CustomerId { get; set; }
        public int Quantity { get; set; }
        public string CustomerName { get; set; }
        public string CustomerCode { get; set; }
        public DateTime DistributionDate { get; set; }
        public Guid OrganizationId { get; set; }
        public Guid QuotaId { get; set; }
        public Guid ProductId { get; set; }
    
    }
}