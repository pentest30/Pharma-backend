using System;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Modules.Inventory.Entities;

namespace GHPCommerce.Modules.Inventory.Repositories
{
    public interface IInventoryRepository : IRepository<InventSum, Guid>
    {
        
    }
}