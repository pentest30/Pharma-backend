using System;
using System.Threading;
using System.Threading.Tasks;
using GHPCommerce.Core.Shared.Contracts.Inventory;

namespace GHPCommerce.Core.Shared.Services
{
    public interface ICacheService
    {
        Task AddInventToCacheAsync(InventSumCreatedEvent notification, CancellationToken cancellationToken );
        Task<int> CreateSequenceNumber<T>(Guid organizationId, Guid docId, int year);
        Task ReleaseQuantitiesAsync(Guid productId, string internalBatchNumber, int quantity);

    }
}