using System;
using System.Collections.Generic;
using GHPCommerce.Core.Shared.Contracts.Common.DTOs;
using GHPCommerce.Core.Shared.Contracts.Tiers.Guest;
using GHPCommerce.Domain.Domain.Events;

namespace GHPCommerce.Core.Shared.Events.Guest
{
    public interface IGuestCreatedEvent
    {
        string Guest { get; set; }
        List<ShoppingCartItemDto> ShoppingCartItemModels { get; set; }
    }

    public class GuestShipCreatedEvent :IEvent, IGuestCreatedEvent
    {
        public string Guest { get; set; }
        public string Email { get; set; }
        public Guid GuestId { get; set; }
        public List<ShoppingCartItemDto> ShoppingCartItemModels { get; set; }
        public AddressDto Address { get; set; }
    }
}
