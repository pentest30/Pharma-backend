using System;
using GHPCommerce.Modules.Inventory.Entities;

namespace GHPCommerce.Modules.Inventory.DTOs
{
    public class StockStateDtoV1
    {
        public Guid Id { get; set; }
        public string  ZoneType { get; set; }
        public string Name { get; set; }
        public EntityStatus StockStatus { get; set; }
       
    }
}