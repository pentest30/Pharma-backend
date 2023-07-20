using GHPCommerce.Domain.Repositories;
using GHPCommerce.Modules.Sales.Entities;
using System;

namespace GHPCommerce.Modules.Sales.Repositories
{
    public interface IDiscountsRepository : IRepository<Discount, Guid>
    {
    }
}
