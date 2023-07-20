using System;

namespace GHPCommerce.Core.Shared.Contracts.Inventory.Dtos
{
    public class InventRQDto
    {
        public Guid ProductId { get; set; }
        public double RemainQnt { get; set; }
        public double QuotaQnt { get; set; }
    }
}