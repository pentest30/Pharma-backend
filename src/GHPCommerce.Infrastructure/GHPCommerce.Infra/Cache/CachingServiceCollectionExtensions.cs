using GHPCommerce.CrossCuttingConcerns.Caching;
using Microsoft.Extensions.DependencyInjection;


namespace GHPCommerce.Infra.Cache
{
    public static class CachingServiceCollectionExtensions
    {
        public static IServiceCollection AddCaches(this IServiceCollection services,CachingOptions options = null)
        {
            services.AddMemoryCache(opt =>
            {
                opt.SizeLimit = options?.InMemory?.SizeLimit;
            });

            var distributedProvider = options?.Distributed?.Provider;

            if (distributedProvider == "InMemory")
            {
                services.AddDistributedMemoryCache(opt =>
                {
                    opt.SizeLimit = options.Distributed?.InMemory?.SizeLimit;
                });
                services.AddSingleton(typeof(ICache), typeof(InMemoryCache));
            }
            else if (distributedProvider == "Redis")
            {
                // uses StackExchange.Redis
                var manager = new RedisConnectionManager(options.Distributed.Redis.Configuration,
                    options.Distributed.Redis.Password);
                services.AddSingleton<IRedisConnectionManager>(manager);
                services.AddSingleton(typeof(ICache), typeof(StackExchangeRedis));
            }

            return services;
        }
    }
}
