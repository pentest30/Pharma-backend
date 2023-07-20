using System;
using System.Collections.Generic;
using GHPCommerce.Domain.Domain.Common;
using GHPCommerce.Domain.Domain.Shared;

namespace GHPCommerce.Domain.Domain.Tiers
{
    public class Guest : AggregateRoot<Guid>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CompanyName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Fax { get; set; }
        public List<Address> Addresses { get; set; }
    }
}
