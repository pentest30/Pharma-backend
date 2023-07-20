using System;
using System.Collections.Generic;
using GHPCommerce.Domain.Domain.Common;

namespace GHPCommerce.Domain.Domain.Tiers
{
    public class Customer : AggregateRoot<Guid>
    {
        public Customer()
        {
            SupplierCustomers = new List<SupplierCustomer>();
        }
        public Organization Organization { get; set; }
        public Guid OrganizationId { get; set; }
        public List<SupplierCustomer> SupplierCustomers { get; set; }
        public string Code { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
    }
}
