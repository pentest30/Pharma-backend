using System;

namespace GHPCommerce.Core.Shared.Contracts.Inventory.Dtos
{
    public class InventDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public Guid BatchId { get; set; }

        public Guid ZoneId { get; set; }
        public string ZoneName { get; set; }

        public Guid StockStateId { get; set; }
        public string StockStateName { get; set; }
        public double PhysicalQuantity { get; set; }
        public double originQuantity { get; set; }

    }
}