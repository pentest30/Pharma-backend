using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GHPCommerce.Core.Shared.Contracts.Cache;
using GHPCommerce.Core.Shared.Contracts.Inventory;
using GHPCommerce.CrossCuttingConcerns.Caching;
using GHPCommerce.Domain.Services;

namespace GHPCommerce.Core.Shared.Services
{
    public class CacheService : ICacheService
    {
        private readonly ICache _cache;
        private readonly ICurrentOrganization _currentOrganization;

        public CacheService(ICache cache, ICurrentOrganization currentOrganization)
        {
            _cache = cache;
            _currentOrganization = currentOrganization;
        }
        public  async Task AddInventToCacheAsync(InventSumCreatedEvent notification, CancellationToken cancellationToken )
        {
            var inventSum = await _cache.GetAsync<InventSumCreatedEvent>(notification.Id, cancellationToken);
            if (inventSum == null)
                await _cache.AddOrUpdateAsync<InventSumCreatedEvent>(notification.Id, notification,cancellationToken);

            else
            {
                inventSum.CachedInventSumCollection.CachedInventSums.AddRange(notification.CachedInventSumCollection
                    .CachedInventSums);
                await _cache.AddOrUpdateAsync<InventSumCreatedEvent>(notification.Id, inventSum,cancellationToken);
            }
        }

        public async Task<int> CreateSequenceNumber<T>(Guid organizationId, Guid docId, int year)
        {
            var key = typeof(T).Name + organizationId;
            var docs = await _cache.GetAsync<DocSequenceNumbers>(key) ?? new DocSequenceNumbers();
            docs.SequenceNumbers.Add(new DocSequenceNumber { DocId = docId, Year =  year});
            await _cache.AddOrUpdateAsync<DocSequenceNumbers>(key, docs);
            return docs.SequenceNumbers.Where(x=>x.Year == year).ToList().Count;
        }

        public async Task ReleaseQuantitiesAsync(Guid productId, string internalBatchNumber, int quantity)
        {
            var supplierId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            var key = productId.ToString() + supplierId;
            try
            {
                //await LockProvider<string>.ProvideLockObject(key).WaitAsync();
                var inventSum = await _cache.GetAsync<InventSumCreatedEvent>(key);
                var index = inventSum.CachedInventSumCollection.CachedInventSums
                    .FindIndex(x =>
                        x.ProductId == productId
                        && x.InternalBatchNumber == internalBatchNumber);
                if (index == -1)
                {
                    return;
                }

                if (inventSum.CachedInventSumCollection.CachedInventSums[index].PhysicalReservedQuantity >=quantity)
                    inventSum.CachedInventSumCollection.CachedInventSums[index].PhysicalReservedQuantity -= quantity;
                else
                    inventSum.CachedInventSumCollection.CachedInventSums[index].PhysicalReservedQuantity = 0;

                await _cache.AddOrUpdateAsync<InventSumCreatedEvent>(key, inventSum);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                //_logger.Error(e.Message);
                //_logger.Error(e.InnerException?.Message);
                //throw;
            }
            finally
            {
                //LockProvider<string>.ProvideLockObject(key).Release();
            }

        }
    }
}