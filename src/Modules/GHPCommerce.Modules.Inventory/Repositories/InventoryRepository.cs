using System;
using GHPCommerce.CrossCuttingConcerns.OS;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Modules.Inventory.Entities;

namespace GHPCommerce.Modules.Inventory.Repositories
{
    public class InventoryRepository : Repository<InventSum, Guid>, IInventoryRepository
    {
        public InventoryRepository(InventoryDbContext dbContext, IDateTimeProvider dateTimeProvider,ICurrentUser currentUser) : base(
            dbContext, dateTimeProvider,currentUser)
        {
        }

    }
}
