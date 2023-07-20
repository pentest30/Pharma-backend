using System;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Modules.Quota.Entities;

namespace GHPCommerce.Modules.Quota.Repositories
{
    public interface IQuotaRequestRepository : IRepository<QuotaRequest, Guid>
    {
        
    }
}