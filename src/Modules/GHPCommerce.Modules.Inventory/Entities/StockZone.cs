using System;
using GHPCommerce.Domain.Domain.Common;

namespace GHPCommerce.Modules.Inventory.Entities
{
    public class StockZone : AggregateRoot<Guid>
    {
        public ZoneType ZoneType { get; set; }
        public Guid ZoneTypeId { get; set; }
        public EntityStatus ZoneState { get; set; }
        public string Name { get; set; }
        public Guid OrganizationId { get; set; }

    }
}
