using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using GHPCommerce.Core.Shared.Contracts.Cache;
using GHPCommerce.CrossCuttingConcerns.Caching;
using Newtonsoft.Json;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;

namespace GHPCommerce.Infra.Cache
{
    public class StackExchangeRedis : ICache
    {
        private readonly IRedisConnectionManager _cacheConnection;
        private static Store _store;
        public RedLockFactory RedLockFactory { get; }
        public StackExchangeRedis(IRedisConnectionManager cacheConnection)
        {
            _cacheConnection = cacheConnection;
            //_store = Get<Store>("GhpCommerceDb");
           // if (_store == null)
           //     _cacheConnection.RedisServer.StringSet("GhpCommerceDb", JsonConvert.SerializeObject(new Store(new List<CachedKey>())));
           //// _store = Get<Store>("GhpCommerceDb");
            var endPoints = new List<RedLockEndPoint>
            {
                new DnsEndPoint("127.0.0.1", 6379)
            };
           
            RedLockFactory = RedLockFactory.Create(endPoints);
           
        }
        private async Task UpdateStore<T>(string key, bool remove = false, DateTime? expiresAt = null)
        {
            return;
            var db = _cacheConnection.RedisServer;
            try
            {
                var item = _store.CachedKeys
                    .ToList()
                    .FirstOrDefault(k => k!=null&& (k.TypeName == typeof(T).Name && k.Key == key));
                if (item == null)
                    _store.CachedKeys
                        .ToList()
                        .Add(new CachedKey(typeof(T).Name, key, expiresAt));
                else if(remove)
                {
                    var index = _store.CachedKeys
                        .ToList()
                        .FindIndex(x => x == item);
                    if (index>=0 )
                        _store.CachedKeys.ToList().RemoveAt(index);
                }
                if(item==null ||remove) 
                    await db.StringSetAsync("GhpCommerceDb", JsonConvert.SerializeObject(_store));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
               // throw;
            }
        }

        public async void AddOrUpdate<T>(string key, object value)
        {
            await UpdateStore<T>(key);
            var db = _cacheConnection.RedisServer;
            //var tran = db.CreateTransaction();
            try
            {
                db.StringSet(key, JsonConvert.SerializeObject(value));
                //await tran.ExecuteAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task AddOrUpdateAsync<T>(string key, object value, CancellationToken token = default)
        {
            await UpdateStore<T>(key);
            var db = _cacheConnection.RedisServer;
            try
            {
                await db.StringSetAsync(key, await  GetJson(value)); 
                 
            }
            catch (Exception e)
            {
                //Console.WriteLine(e);
                //throw;
            }
            
        }
        private static async Task<string> GetJson(object obj)
        {
            using (var stream = new MemoryStream())
            {
                await System.Text.Json.JsonSerializer.SerializeAsync(stream, obj, obj.GetType());
                stream.Position = 0;
                using var reader = new StreamReader(stream);
                return await reader.ReadToEndAsync();
            }
        }

        public async Task AddOrUpdateWithTransAsync<T>(string key, object value, CancellationToken token = default)
        {
            await UpdateStore<T>(key);
            var db = _cacheConnection.RedisServer;
            var tran = db.CreateTransaction();
            try
            {
                var t = tran.StringSetAsync(key, JsonConvert.SerializeObject(value));
                if (await tran.ExecuteAsync().ConfigureAwait(false))
                    await t.ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task AddOrUpdateAsync<T>(string key, object value, DateTimeOffset expireDateTime,
            CancellationToken token = default)
        {
            await UpdateStore<T>(key);
            var db = _cacheConnection.RedisServer;

            try
            {
                await db.StringSetAsync(key, JsonConvert.SerializeObject(value));

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }

        public T Get<T>(string keyName)
        {
            try
            {
                string data = null;
                try
                {
                    var db = _cacheConnection.RedisServer;
                    //var tran = db.CreateTransaction();
                    data = db.StringGet(keyName);
                   // tran.Execute();
                }
                catch (Exception ex)
                {
                    //TODO: need logging here
                }

                return data == null ? default : JsonConvert.DeserializeObject<T>(data);
            }
            catch (Exception ex)
            {
                //TODO: should log this
                return default;
            }
        }

        public async Task<T> GetAsync<T>(string keyName, CancellationToken token = default)
        {
            try
            {
                string data = null;
                try
                {
                    var db = _cacheConnection.RedisServer;
                    data =await db.StringGetAsync(keyName);
                    // await tran.ExecuteAsync();
                }
                catch (Exception ex)
                {
                    //TODO: need logging here
                }
                return data == null ? default : await Task.Run(()=> JsonConvert.DeserializeObject<T>(data), token);
            }
            catch (Exception ex)
            {
                //TODO: should log this
                return default;
            }
        }
       

        public async Task<T> GetWithTransAsync<T>(string keyName, CancellationToken token = default)
        {

            try
            {
                string data = null;
                try
                {
                    var db = _cacheConnection.RedisServer;
                    var tran = db.CreateTransaction();
                    var task = tran.StringGetAsync(keyName);
                    if (await tran.ExecuteAsync())
                        data = await task;
                    // await tran.ExecuteAsync();
                }
                catch (Exception ex)
                {
                    //TODO: need logging here
                }

                return data == null ? default : JsonConvert.DeserializeObject<T>(data);
            }
            catch (Exception ex)
            {
                //TODO: should log this
                return default;
            }
        }

        public T Get<T>(string keyName, Func<T> queryFunction)
        {
            return Get<T>(keyName, 60, queryFunction);
        }

        public T Get<T>(string keyName, int expireTimeInMinutes, Func<T> queryFunction)
        {
            try
            {
                string data = null;

                try
                {
                    data = _cacheConnection.RedisServer.StringGet(keyName);
                }
                catch (Exception ex)
                {
                    // TODO: logging here
                }

                if (data == null)
                {
                    var result = queryFunction();

                    if (result != null)
                    {
                        try
                        {
                            _cacheConnection.RedisServer.StringSet(keyName, JsonConvert.SerializeObject(result),
                                new TimeSpan(0, expireTimeInMinutes, 0));
                        }
                        catch (Exception ex)
                        {
                            // TODO: logging here
                        }
                    }

                    return result;
                }

                return JsonConvert.DeserializeObject<T>(data);
            }
            catch (Exception ex)
            {
                // TODO: logging here
                return default;
            }
        }

        public void Expire<T>(string keyName)
        {
            try
            {
                UpdateStore<T>(keyName, true);
                _cacheConnection.RedisServer.KeyDelete(keyName);
            }
            catch (Exception ex)
            {
                // TODO: logging here
            }
        }

        public async Task ExpireAsync<T>(string keyName, CancellationToken token = default)
        {
            try
            {
                await UpdateStore<T>(keyName, true);
                var db = _cacheConnection.RedisServer;
                await db.KeyDeleteAsync(keyName);
                // if (await tran.ExecuteAsync()) await t;
            }
            catch (Exception ex)
            {
                // TODO: logging here
            }
        }

        public async Task ExpireWithTransAsync<T>(string keyName, CancellationToken token = default)
        {
            try
            {
                await UpdateStore<T>(keyName, true);

                var db = _cacheConnection.RedisServer;
                var tran = db.CreateTransaction();

                var t = tran.KeyDeleteAsync(keyName);
                if (await tran.ExecuteAsync()) await t;
            }
            catch (Exception ex)
            {
                // TODO: logging here
            }

        }

        public  async Task<IEnumerable<T>> GetAsync<T>(string[] keyNames, CancellationToken token = default)
        {
            try
            {
                var result = new List<T>();
                try
                {
                    var  data = default(T);

                    foreach (var keyName in keyNames)
                    {
                        data =  Get<T>(keyName);
                        if (data != null) result.Add(data);
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
