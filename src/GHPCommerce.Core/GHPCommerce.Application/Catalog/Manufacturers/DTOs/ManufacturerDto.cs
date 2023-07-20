using System;
using System.Collections.Generic;
using GHPCommerce.Core.Shared.Contracts.Common.DTOs;
using GHPCommerce.Domain.Domain.Catalog;

namespace GHPCommerce.Application.Catalog.Manufacturers.DTOs
{
    public class ManufacturerDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public List<AddressDto> Addresses { get; set; }
        public List<PhoneNumberDto> PhoneNumbers { get; set; }
        public List<EmailDto> Emails  { get; set; }
        public ICollection<Product> Products { get; set; }
    }

    
}