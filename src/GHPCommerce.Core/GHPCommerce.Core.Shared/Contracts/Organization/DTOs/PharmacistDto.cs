using System;
using GHPCommerce.Core.Shared.Contracts.Common.DTOs;

namespace GHPCommerce.Core.Shared.Contracts.Organization.DTOs
{
    [Serializable]
    public class PharmacistDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public AddressDto Address { get; set; }
        public string Email { get; set; }
    }
}
