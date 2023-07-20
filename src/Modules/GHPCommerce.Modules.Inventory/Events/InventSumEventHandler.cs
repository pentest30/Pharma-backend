using System.Threading;
using System.Threading.Tasks;
using GHPCommerce.Core.Shared.Contracts.Inventory;
using GHPCommerce.CrossCuttingConcerns.Caching;
using GHPCommerce.Domain.Domain.Events;

namespace GHPCommerce.Modules.Inventory.Events
{
    public class InventSumEventHandler : IEventHandler<InventSumCreatedEvent>
    {
        private readonly ICache _distributedCache;

        public InventSumEventHandler(ICache distributedCache)
        {
            _distributedCache = distributedCache;
        }
        public async Task Handle(InventSumCreatedEvent notification, CancellationToken cancellationToken)
        {

            var inventSum = await _distributedCache.GetAsync<InventSumCreatedEvent>(notification.Id, cancellationToken);
            if (inventSum == null)
            {
                await _distributedCache.AddOrUpdateAsync<InventSumCreatedEvent>(notification.Id, notification, cancellationToken);
            }
            else
            {
                inventSum.CachedInventSumCollection.CachedInventSums.AddRange(notification.CachedInventSumCollection.CachedInventSums);
                await _distributedCache.AddOrUpdateAsync<InventSumCreatedEvent>(notification.Id, inventSum, cancellationToken);
            }
        }
      

    }
}
