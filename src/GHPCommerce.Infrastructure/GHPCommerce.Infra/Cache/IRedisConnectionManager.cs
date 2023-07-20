using StackExchange.Redis;

namespace GHPCommerce.Infra.Cache
{
    public interface IRedisConnectionManager
    {
        IDatabase RedisServer { get; }
        
    }
}
