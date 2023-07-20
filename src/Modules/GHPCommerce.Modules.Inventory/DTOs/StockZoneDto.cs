using System;

namespace GHPCommerce.Modules.Inventory.DTOs
{
    public class StockZoneDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string ZoneType { get; set; }
    }
}