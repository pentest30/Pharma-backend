using System;
using GHPCommerce.Domain.Repositories;

namespace GHPCommerce.Modules.PreparationOrder.Repositories
{
    public interface IPreparationOrderRepository : IRepository<Entities.PreparationOrder, Guid>
    {
    }
}
