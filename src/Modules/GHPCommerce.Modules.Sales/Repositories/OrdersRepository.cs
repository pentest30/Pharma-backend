using System;
using GHPCommerce.CrossCuttingConcerns.OS;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Modules.Sales.Entities;

namespace GHPCommerce.Modules.Sales.Repositories
{
    public class OrdersRepository : Repository<Order, Guid>, IOrdersRepository
    {
        public OrdersRepository(SalesDbContext dbContext, IDateTimeProvider dateTimeProvider,ICurrentUser currentUser) : base(dbContext, dateTimeProvider,currentUser)
        {
        }
    }
}
