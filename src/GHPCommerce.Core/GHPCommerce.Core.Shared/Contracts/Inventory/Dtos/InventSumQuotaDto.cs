using System;

namespace GHPCommerce.Core.Shared.Contracts.Inventory.Dtos
{
    public class InventSumQuotaDto
    {
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; }
        public Guid ProductId { get; set; }
        public string OrganizationName { get; set; }
        public string ProductCode { get; set; }
        public string ProductFullName { get; set; }
        public string InternalBatchNumber { get; set; }
        public double? PhysicalDispenseQuantity { get; set; }
    }
}