using GHPCommerce.Domain.Domain.Common;
using System;
using System.Collections.Generic;

namespace GHPCommerce.Domain.Domain.Catalog
{
    public class ZoneGroup : AggregateRoot<Guid>
    {
        public string Name { get; set; }
        public int Order { get; set; }
        public string Description { get; set; }
        public string Printer { get; set; }
        public List<PickingZone> PickingZones { get; set; }
        public ZoneGroup(Guid id, string name, string description, int order, string printer)
        {
            Id = id;
            Name = name;
            Description = description;
            Order = order;
            Printer = printer;
        }
    }
}
