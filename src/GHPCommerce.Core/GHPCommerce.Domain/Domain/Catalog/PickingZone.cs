using System;
using System.Collections.Generic;
using GHPCommerce.Domain.Domain.Common;

namespace GHPCommerce.Domain.Domain.Catalog
{
    public class PickingZone : AggregateRoot<Guid>
    {
      
        public PickingZone()
        {
            Products = new List<Product>();
        }

        public PickingZone(Guid id, string name,Guid groupZoneId)
        {
            Id = id;
            Name = name;
            ZoneGroupId = groupZoneId;
        }
        public string Name { get; set; }
        public int Order { get; set; }
        public ZoneType ZoneType { get; set; }
        public Guid? ZoneGroupId { get; set; }
        public ZoneGroup ZoneGroup { get; set; }
        public List<Product> Products { get; set; }
    }
}
