using System;
using GHPCommerce.Domain.Domain.Common;

namespace GHPCommerce.Modules.Inventory.Entities
{
    public class StockState : AggregateRoot<Guid>
    {
        public ZoneType ZoneType { get; set; }
        public Guid ZoneTypeId { get; set; }
        public string Name { get; set; }
        public EntityStatus StockStatus { get; set; }
        public Guid OrganizationId { get; set; }

    }
}
