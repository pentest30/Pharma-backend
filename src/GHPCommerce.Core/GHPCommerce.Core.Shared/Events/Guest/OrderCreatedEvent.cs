using System.Collections.Generic;
using GHPCommerce.Core.Shared.Contracts.Tiers.Guest;
using GHPCommerce.Domain.Domain.Events;

namespace GHPCommerce.Core.Shared.Events.Guest
{
    public class OrderCreatedEvent :IGuestCreatedEvent, IEvent
    {
        public string Guest { get; set; }
        public List<ShoppingCartItemDto> ShoppingCartItemModels { get; set; }
    }
}
