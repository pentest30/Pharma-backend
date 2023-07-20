using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GHPCommerce.CrossCuttingConcerns.Caching;
using GHPCommerce.CrossCuttingConcerns.ExtensionMethods;
using Microsoft.Extensions.Caching.Distributed;

namespace GHPCommerce.Infra.Cache
{
    public class InMemoryCache : ICache
    {
        private readonly IDistributedCache _distributedCache;

        public InMemoryCache(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }

        public void AddOrUpdate<T>(string key, object value)
        {
             _distributedCache.Set(key, value.Serialize());

        }

        public Task AddOrUpdateAsync<T>(string key, object value, CancellationToken token = default)
        {
            return _distributedCache.SetAsync(key, value.Serialize(), token: token);
        }

        public Task AddOrUpdateWithTransAsync<T>(string key, object value, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public Task AddOrUpdateAsync<T>(string key, object value, DateTimeOffset expireDateTime, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public T Get<T>(string keyName)
        {
            throw new NotImplementedException();
        }

        public async Task<T> GetAsync<T>(string keyName, CancellationToken token = default)
        {
            try
            {
                byte[] data = null;
                try
                {
                    data = await _distributedCache.GetAsync(keyName, token);
                }
                catch (Exception)
                {
                    //TODO: need logging here
                }

                return data == null ? default : data.Deserialize<T>();
            }
            catch (Exception)
            {
                //TODO: should log this
                return default;
            }
        }

        public Task<T> GetWithTransAsync<T>(string keyName, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public void Expire<T>(string keyName)
        {
            throw new NotImplementedException();
        }

        public Task ExpireAsync<T>(string keyName, CancellationToken token = default)
        {
            try
            {
                return _distributedCache.RemoveAsync(keyName, token);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public Task ExpireWithTransAsync<T>(string keyName, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<T>> GetAsync<T>(string[] keyNames, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }
    }
}
