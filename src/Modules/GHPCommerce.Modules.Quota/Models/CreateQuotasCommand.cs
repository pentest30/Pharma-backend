using System;
using System.Collections.Generic;
using GHPCommerce.Modules.Quota.Commands;

namespace GHPCommerce.Modules.Quota.Models
{
    public class CreateQuotasModel 
    {
        public Guid ProductId { get; set; }
        public int QuantityReserved { get; set; }
        public List<CreateQuotaCommand> Quotas { get; set; }
        public Guid RequestId { get; set; }
    }
}