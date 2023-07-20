using System;
using GHPCommerce.Domain.Domain.Tiers;
using GHPCommerce.Domain.Repositories;

namespace GHPCommerce.Modules.Sales.Repositories
{
    public interface IGuestRepository : IRepository<Guest, Guid>
    {
        
    }
}