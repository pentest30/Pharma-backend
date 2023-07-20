using System;
using System.Collections.Generic;
using GHPCommerce.Domain.Domain.Common;

namespace GHPCommerce.Domain.Domain.Tiers
{
    public class Supplier :AggregateRoot<Guid>
    {
        public Supplier()
        {
            SupplierCustomers = new List<SupplierCustomer>();
        }
        public Organization Organization { get; set; }
        public Guid OrganizationId { get; set; }
        public List<SupplierCustomer> SupplierCustomers { get; set; }
    }
}
