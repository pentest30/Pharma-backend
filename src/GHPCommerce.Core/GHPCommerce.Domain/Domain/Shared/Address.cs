using System;
using GHPCommerce.Domain.Domain.Catalog;
using GHPCommerce.Domain.Domain.Common;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Domain.Tiers;

namespace GHPCommerce.Domain.Domain.Shared
{
   
    public class Address : Entity<Guid>, ICloneable
    {
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Township { get; set; }
        public string ZipCode { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set; }
        public string Country { get; set; }
        public bool Main { get; set; }
        public bool Shipping { get; set; }
        public bool Billing { get; set; }
        public Manufacturer Manufacturer { get; set; }
        public Organization Organization { get; set; }
        public Guid? OrganizationId { get; set; }
        public Guid? ManufacturerId { get; set; }
        public Guid? GuestId { get; set; }
        public Guest Guest { get; set; }
        public Guid? UserId { get; set; }
        public User User { get; set; }
        public Address()
        {
        }

       

        public bool Equals(Address other)
        {
            return Street == other.Street && City == other.City && State == other.State && Township == other.Township &&
                   ZipCode == other.ZipCode &&  Country == other.Country  ;
        }

       

        public Address(string street, string city, string zipCode, float latitude, float longitude, string country,
            bool main, bool billing, bool shipping, string state, string township)
        {
            Street = street;
            City = city;
            ZipCode = zipCode;
            Latitude = latitude;
            Longitude = longitude;
            Country = country;
            Main = main;
            Billing = billing;
            Shipping = shipping;
            State = state;
            Township = township;
        }

        public object Clone()
        {
            return MemberwiseClone();
            
        }
    }
}
