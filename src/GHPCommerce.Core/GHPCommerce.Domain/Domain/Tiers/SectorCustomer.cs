using System;
using System.Collections.Generic;
using GHPCommerce.Domain.Domain.Common;

namespace GHPCommerce.Domain.Domain.Tiers
{
    public class SectorCustomer : AggregateRoot<Guid>
    {
        public int ExternalId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public Guid OrganizationId { get; set; }
        public Organization Organization { get; set; }
        public List<SupplierCustomer> SupplierCustomers { get; set; }

    }
}
