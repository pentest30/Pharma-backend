using System;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Modules.Sales.Entities;

namespace GHPCommerce.Modules.Sales.Repositories
{
    public interface IOrdersRepository : IRepository<Order, Guid>
    { }
}