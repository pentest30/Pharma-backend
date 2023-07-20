using System;
using GHPCommerce.Domain.Domain.Commands;
using Microsoft.AspNetCore.Mvc;

namespace GHPCommerce.Modules.Inventory.Queries
{
    [BindProperties]
    public class InventoryDimensionExistsQuery:ICommand<bool>
    {

        public Guid? OrganizationId { get; set; }
        public Guid ProductId { get; set; }
        public string VendorBatchNumber { get; set; }
        public string InternalBatchNumber { get; set; } 
        
        public string Color { get; set; }
        public string Size { get; set; }
        
        public bool IsPublic { get; set; }
        public Guid? SiteId { get; set; }
        
        public Guid? WarehouseId { get; set; }
    }
}
