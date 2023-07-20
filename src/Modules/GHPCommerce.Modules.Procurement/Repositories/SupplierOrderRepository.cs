using System;
using GHPCommerce.CrossCuttingConcerns.OS;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Modules.Procurement.Entities;

namespace GHPCommerce.Modules.Procurement.Repositories
{
    public class SupplierOrderRepository : Repository<SupplierOrder, Guid>, ISupplierOrderRepository
    {
        public SupplierOrderRepository(ProcurementDbContext dbContext, IDateTimeProvider dateTimeProvider, ICurrentUser currentUser) : base(dbContext, dateTimeProvider, currentUser)
        {
        }
    }
}