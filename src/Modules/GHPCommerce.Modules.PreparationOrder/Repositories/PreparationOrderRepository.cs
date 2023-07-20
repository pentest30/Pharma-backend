using System;
using GHPCommerce.CrossCuttingConcerns.OS;
using GHPCommerce.Domain.Domain.Identity;
namespace GHPCommerce.Modules.PreparationOrder.Repositories
{
    public class PreparationOrderRepository : Repository<Entities.PreparationOrder, Guid>, IPreparationOrderRepository
    {
        public PreparationOrderRepository(PreparationOrderDbContext dbContext, IDateTimeProvider dateTimeProvider, ICurrentUser currentUser)
           : base(dbContext, dateTimeProvider, currentUser)
        {
        }
    }
}
