using System;
using System.Collections.Generic;
using AutoMapper;
using FluentValidation.Results;
using GHPCommerce.Core.Shared.Contracts.Tiers.Guest;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Tiers.Guests.Commands
{
    public class CreateGuestPickupCommand : ICommand<ValidationResult>
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CompanyName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public Guid VendorId { get; set; }
        public string CustomerName { get; set; }
        public List<ShoppingCartItemDto> ShoppingCartItems { get; set; }
    }

    public class GuestMappingConfiguration : Profile
    {
        public GuestMappingConfiguration()
        {
            CreateMap<Domain.Domain.Tiers.Guest, CreateGuestPickupCommand>()
                //.ForSourceMember(x=>x.ShoppingCartItems, o => o.DoNotValidate())
                .ReverseMap();
        }
    }
}
