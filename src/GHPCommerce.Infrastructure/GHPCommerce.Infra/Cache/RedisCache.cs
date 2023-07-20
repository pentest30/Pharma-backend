using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GHPCommerce.Core.Shared.Contracts.Cache;
using GHPCommerce.CrossCuttingConcerns.Caching;
using GHPCommerce.CrossCuttingConcerns.ExtensionMethods;
using Microsoft.Extensions.Caching.Distributed;

namespace GHPCommerce.Infra.Cache
{
    public class RedisCache :ICache
    {
        private readonly IDistributedCache _distributedCache;
        private static Store _store;
        private static object obj = new  object();
        public RedisCache(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
            _store = Get<Store>("GhpCommerceDb");
            if (_store == null)
                _distributedCache.SetAsync("GhpCommerceDb", new Store(new List<CachedKey>()).Serialize().Compress().GetAwaiter().GetResult());
            _store = Get<Store>("GhpCommerceDb");
        }
        private  void UpdateStore<T>(string key,bool remove=false,DateTime? expiresAt =null)
        {
            var index = _store.CachedKeys.FindIndex(k => k.TypeName == typeof(T).Name && k.Key == key);
            if (index < 0)
            {
                _store.CachedKeys.Add(new CachedKey(typeof(T).Name,key,expiresAt));
                AddOrUpdateAsync<Store>("GhpCommerceDb", _store);
            }
            else
            {
                if (remove)
                {
                    _store.CachedKeys.RemoveAt(index);
                    AddOrUpdateAsync<Store>("GhpCommerceDb", _store);
                }
            }
        }

        public void AddOrUpdate<T>(string key, object value)
        {
            UpdateStore<T>(key);

             _distributedCache.Set(key, value.Serialize().Compress().GetAwaiter().GetResult());

        }

        public  Task AddOrUpdateAsync<T>(string key, object value, CancellationToken token = default)
        {
             UpdateStore<T>(key);
            
            return  _distributedCache.SetAsync(key, value.Serialize().Compress().GetAwaiter().GetResult(), token: token);

        }

        public Task AddOrUpdateWithTransAsync<T>(string key, object value, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public Task AddOrUpdateAsync<T>(string key, object value, DateTimeOffset expireDateTime, CancellationToken token = default)
        {
            UpdateStore<T>(key);
            return _distributedCache.SetAsync(key, value.Serialize().Compress().GetAwaiter().GetResult() , new DistributedCacheEntryOptions {AbsoluteExpiration = expireDateTime},token);
        }

        public T Get<T>(string keyName)
        {
            lock (obj)
            {
                try
                {
                    byte[] data = null;
                    try
                    {
                        data = _distributedCache.Get(keyName);
                    }
                    catch (Exception)
                    {
                        //TODO: need logging here
                    }

                    return data == null ? default : data.Decompress().GetAwaiter().GetResult().Deserialize<T>();
                }
                catch (Exception)
                {
                    //TODO: should log this
                    return default;
                }
            }

           
           
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

                return data == null ? default : (await data.Decompress()).Deserialize<T>();
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
            try
            {
                UpdateStore<T>(keyName,true);
                _distributedCache.Remove(keyName);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public Task ExpireAsync<T>(string keyName, CancellationToken token = default)
        {
            try
            {
                UpdateStore<T>(keyName,true);
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

        public async Task<IEnumerable<T>> GetAsync<T>(string[] keyNames, CancellationToken token = default)
        {
            try
            {
                var result = new List<T>();
                try
                {
                    byte[] data = null;
                    
                    foreach (var keyName in keyNames)
                    {
                        data = await _distributedCache.GetAsync(keyName, token);
                       if(data!=null) result.Add((await data.Decompress()).Deserialize<T>());
                    }
                   
                }
                catch (Exception)
                {
                    //TODO: need logging here
                }

                return result;
            }
            catch (Exception)
            {
                //TODO: should log this
                return default;
            }
        }
    }
}
