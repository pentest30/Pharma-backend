using System.Threading;
using System.Threading.Tasks;
using GHPCommerce.Core.Shared.Contracts.Inventory;
using GHPCommerce.CrossCuttingConcerns.Caching;

namespace GHPCommerce.Modules.Inventory
{
    public static class CommonHelper
    {
        public  static async Task AddInventToCache(ICache cache, InventSumCreatedEvent notification, CancellationToken cancellationToken)
        {
            var inventSum = await cache.GetAsync<InventSumCreatedEvent>(notification.Id, cancellationToken);
            if (inventSum == null)
                await cache.AddOrUpdateAsync<InventSumCreatedEvent>(notification.Id, notification,cancellationToken);

            else
            {
                inventSum.CachedInventSumCollection.CachedInventSums.AddRange(notification.CachedInventSumCollection
                    .CachedInventSums);
                await cache.AddOrUpdateAsync<InventSumCreatedEvent>(notification.Id, inventSum,cancellationToken);
            }
        }
        
    }
}