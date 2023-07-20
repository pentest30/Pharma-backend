using GHPCommerce.Modules.Inventory.Entities;
using System;

namespace GHPCommerce.Modules.Inventory.DTOs.StockState
{
    public class StockStateDto
    {
        public Guid Id { get; set; }
        public ZoneType ZoneType { get; set; }
        public Guid ZoneTypeId { get; set; }
        public string Name { get; set; }
        public EntityStatus StockStatus { get; set; }
    }
}
