using System;
using GHPCommerce.CrossCuttingConcerns.OS;
using GHPCommerce.Domain.Domain.Identity;

namespace GHPCommerce.Modules.Quota.Repositories
{
    public class QuotaRepository :Repository<Entities.Quota, Guid>, IQuotaRepository
    {
        public QuotaRepository(QuotaDbContext dbContext, IDateTimeProvider dateTimeProvider, ICurrentUser currentUser) 
            : base(dbContext, dateTimeProvider, currentUser)
        {
        }
    }
}
