using System;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Modules.Procurement.Entities;

namespace GHPCommerce.Modules.Procurement.Repositories
{
    public interface ISupplierOrderRepository : IRepository<SupplierOrder, Guid>
    {
        
    }
}