using System;
using GHPCommerce.Domain.Domain.Common;

namespace GHPCommerce.Modules.Quota.Entities
{
    public class QuotaTransaction:  Entity<Guid>
    {
        public Quota Quota { get; set; }
        public Guid QuotaId { get; set; }
        public Guid CustomerId { get; set; }
        public int Quantity { get; set; }
        public string CustomerName { get; set; }
        public string CustomerCode { get; set; }
    }
}