using System;
using GHPCommerce.Domain.Domain.Shared;

namespace GHPCommerce.Core.Shared.Contracts.Common.DTOs
{
    [Serializable]
    public class AddressDto
    {
        public AddressDto()
        {
            
        }
        public AddressDto(Address address)
        {
            if (address == null) return;
            Id = address.Id;
            State = address.State;
            City = address.City;
            Township = address.Township;
            ZipCode = address.ZipCode;
            Latitude = address.Latitude;
            Longitude = address.Longitude;
            Street = address.Street;
            Country = address.Country;
        }
        public Guid Id { get; set; }
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
    }
}
