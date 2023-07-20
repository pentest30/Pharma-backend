using System;
using System.Collections.Generic;
using FluentValidation.Results;
using GHPCommerce.Core.Shared.Contracts.Tiers.Guest;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Tiers.Guests.Commands
{
    public class CreateGuestShipCommand : ICommand<ValidationResult>
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CompanyName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
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
        public List<ShoppingCartItemDto> ShoppingCartItems { get; set; }
    }
}
