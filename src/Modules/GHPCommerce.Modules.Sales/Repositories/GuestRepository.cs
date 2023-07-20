using System;
using GHPCommerce.CrossCuttingConcerns.OS;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Domain.Tiers;

namespace GHPCommerce.Modules.Sales.Repositories
{
    public class GuestRepository :  Repository<Guest, Guid>, IGuestRepository
    {
        public GuestRepository(SalesDbContext dbContext, IDateTimeProvider dateTimeProvider, ICurrentUser currentUser) : base(dbContext, dateTimeProvider, currentUser)
        {
        }
    }
}
