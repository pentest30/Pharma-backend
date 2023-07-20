using System;
using GHPCommerce.Domain.Repositories;

namespace GHPCommerce.Modules.Quota.Repositories
{
    public interface IQuotaRepository : IRepository<Entities.Quota, Guid>
    {
        
    }
}