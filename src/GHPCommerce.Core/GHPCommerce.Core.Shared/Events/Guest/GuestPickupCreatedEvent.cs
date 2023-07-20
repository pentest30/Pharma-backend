using System;
using System.Collections.Generic;
using GHPCommerce.Core.Shared.Contracts.Tiers.Guest;
using GHPCommerce.Domain.Domain.Events;

namespace GHPCommerce.Core.Shared.Events.Guest
{
    [Serializable]
    public class GuestPickupCreatedEvent :IEvent, IGuestCreatedEvent
    {
        public Guid GuestId { get; set; }
        public Guid VendorId { get; set; }
        public string CustomerName { get; set; }
        public string Guest { get; set; }
        public string Email { get; set; }
        public List<ShoppingCartItemDto> ShoppingCartItemModels { get; set; }
    }
}
