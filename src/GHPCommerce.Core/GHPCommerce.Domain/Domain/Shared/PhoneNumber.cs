using System;
using GHPCommerce.Domain.Domain.Catalog;
using GHPCommerce.Domain.Domain.Common;
using GHPCommerce.Domain.Domain.Tiers;

namespace GHPCommerce.Domain.Domain.Shared
{
   public class PhoneNumber: Entity<Guid>, ICloneable
   {
        public string CountryCode { get; set; }
        public string Number { get; set; }
        public Boolean IsFax { get; set; }
        public Manufacturer Manufacturer { get; set; }
        public Organization Organization { get; set; }
        public Guid? OrganizationId { get; set; }
        public Guid? ManufacturerId { get; set; }
        public Guid? GuestId { get; set; }
        public PhoneNumber()
        {
            
        }

        public PhoneNumber(string countryCode, string number, Boolean isFax)
        {
            CountryCode = countryCode;
            Number = number;
            IsFax = isFax;
        }


        public object Clone()
        {
            return MemberwiseClone();
        }
   }
}