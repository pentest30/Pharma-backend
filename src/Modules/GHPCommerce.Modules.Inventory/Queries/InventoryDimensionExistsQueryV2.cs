using System;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Modules.Inventory.Queries
{
    public class InventoryDimensionExistsQueryV2 : ICommand<bool>
    {

        public Guid? OrganizationId { get; set; }
        public string ProductCode { get; set; }
        public string VendorBatchNumber { get; set; }
        public string InternalBatchNumber { get; set; }

        public string Color { get; set; }
        public string Size { get; set; }

        public bool IsPublic { get; set; }
        public Guid? SiteId { get; set; }

        public Guid? WarehouseId { get; set; }
    }
}
