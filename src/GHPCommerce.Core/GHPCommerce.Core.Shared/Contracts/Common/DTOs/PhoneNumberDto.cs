using System;

namespace GHPCommerce.Core.Shared.Contracts.Common.DTOs
{
    public class PhoneNumberDto
    {
        public Guid Id { get; set; }
        public string CountryCode { get; set; }
        public string Number { get; set; }
        public Boolean IsFax { get; set; }
    }
}
