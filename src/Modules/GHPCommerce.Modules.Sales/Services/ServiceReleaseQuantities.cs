using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GHPCommerce.Core.Shared.Contracts.Inventory;
using GHPCommerce.Core.Shared.Services;
using GHPCommerce.CrossCuttingConcerns.Caching;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Infra.OS;
using GHPCommerce.Modules.Sales.DTOs;
using GHPCommerce.Modules.Sales.Queries;
using NLog;

namespace GHPCommerce.Modules.Sales.Services
{
    public class ServiceReleaseQuantities : IServiceReleaseQuantities
    {
        private readonly ICache _redisCache;
        private readonly Logger _logger;
        private readonly ICommandBus _commandBus;

        public ServiceReleaseQuantities(ICache redisCache, Logger logger)
        {
            _redisCache = redisCache;
            _logger = logger;
        }
        
        public async Task ReleaseQuantities(Guid productId, string internalBatchNumber, int quantity , Guid supplierId)
        {
            var key = productId.ToString() + supplierId;
            try
            {
                await LockProvider<string>.ProvideLockObject(key).WaitAsync();
                var inventSum = await _redisCache.GetAsync<InventSumCreatedEvent>(key);
                var index = inventSum.CachedInventSumCollection.CachedInventSums
                    .FindIndex(x =>
                        x.ProductId == productId
                        && x.InternalBatchNumber == internalBatchNumber);
                if (index == -1)
                {
                    return;
                }

                if (inventSum.CachedInventSumCollection.CachedInventSums[index].PhysicalReservedQuantity >=
                    quantity)
                    inventSum.CachedInventSumCollection.CachedInventSums[index].PhysicalReservedQuantity -=
                        quantity;
                else
                    inventSum.CachedInventSumCollection.CachedInventSums[index].PhysicalReservedQuantity = 0;

                await _redisCache.AddOrUpdateAsync<InventSumCreatedEvent>(key, inventSum);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                _logger.Error(e.Message);
                _logger.Error(e.InnerException?.Message);
                //throw;
            }
            finally
            {
                LockProvider<string>.ProvideLockObject(key).Release();
            }
        }
    }
}