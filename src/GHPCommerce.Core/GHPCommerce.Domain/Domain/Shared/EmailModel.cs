using System;
using GHPCommerce.Domain.Domain.Catalog;
using GHPCommerce.Domain.Domain.Common;
using GHPCommerce.Domain.Domain.Tiers;

namespace GHPCommerce.Domain.Domain.Shared
{
   
    public class EmailModel : Entity<Guid>, ICloneable
    {
        public EmailModel()
        {

        }
        public string Email { get; set; }
        public Manufacturer Manufacturer { get; set; }
        public Organization Organization { get; set; }
        public Guid? OrganizationId { get; set; }
        public Guid? ManufacturerId { get; set; }
        public Guid? GuestId { get; set; }
        public EmailModel(string email)
        {
            Email = email;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
