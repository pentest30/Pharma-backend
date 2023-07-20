using System;
using GHPCommerce.Domain.Domain.Common;

namespace GHPCommerce.Modules.Inventory.Entities
{
    public class ZoneType : AggregateRoot<Guid>
    {
        public string Name { get; set; }
        
    }
}
