using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GHPCommerce.CrossCuttingConcerns.Caching;

namespace GHPCommerce.Infra.OS
{
   public   class AsyncLock
   {
      private readonly ICache _redisCache;
      private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1,1);

      public AsyncLock(ICache redisCache)
      {
         _redisCache = redisCache;
      }

      public async Task LockAsync(string key)
      {
         var mainKey = $"_sema_{key}";
         var r = await _redisCache.GetAsync<string>(mainKey);
         if (r == null)
         {
            await _semaphoreSlim.WaitAsync();
            await _redisCache.AddOrUpdateAsync<string>(mainKey, key);
         }
      }

      private async Task<string> GetOrAddAsync(string key, string mainKey)
      {
         var r = await _redisCache.GetAsync<string>(mainKey);
         if (r == null)
         {
            await _redisCache.AddOrUpdateAsync<string>(mainKey, key);
            return key;
         }
         return String.Empty;
      }
   }
}
