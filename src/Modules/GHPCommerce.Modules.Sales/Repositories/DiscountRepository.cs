using GHPCommerce.CrossCuttingConcerns.OS;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Modules.Sales.Entities;
using System;

namespace GHPCommerce.Modules.Sales.Repositories
{
    public class DiscountRepository : Repository<Discount, Guid>, IDiscountsRepository
    {
        public DiscountRepository(SalesDbContext dbContext, IDateTimeProvider dateTimeProvider, ICurrentUser currentUser) : base(dbContext, dateTimeProvider, currentUser)
        {
        }
    }
}
