using System;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Modules.Procurement.Entities;

namespace GHPCommerce.Modules.Procurement.Repositories
{
    public interface IDeliveryReceiptRepository : IRepository<DeliveryReceipt, Guid>
    {
    }
}