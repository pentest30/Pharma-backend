using System;
using GHPCommerce.CrossCuttingConcerns.OS;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Modules.Quota.Entities;

namespace GHPCommerce.Modules.Quota.Repositories
{
    public class QuotaRequestRepository : Repository<QuotaRequest, Guid>, IQuotaRequestRepository
    {
        public QuotaRequestRepository(QuotaDbContext dbContext, IDateTimeProvider dateTimeProvider, ICurrentUser currentUser) 
            : base(dbContext, dateTimeProvider, currentUser)
        {
        }
    }
}