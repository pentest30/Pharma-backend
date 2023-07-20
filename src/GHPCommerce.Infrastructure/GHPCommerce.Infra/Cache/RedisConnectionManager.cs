using System;
//using Medallion.Threading.Redis;
using StackExchange.Redis;

namespace GHPCommerce.Infra.Cache
{
    public class RedisConnectionManager : IRedisConnectionManager
    {
        private static string _serverAddress;
        static string  _password;
       
        public RedisConnectionManager(string serverAddress, string password)
        {
            _serverAddress = serverAddress;
            _password = password;
            lock (Locker)
            {
                _conn ??= new Lazy<ConnectionMultiplexer>(
                    () => ConnectionMultiplexer.Connect(ConfigOptions.Value));
            }
           
        }

        private static readonly Lazy<ConfigurationOptions> ConfigOptions = new(() =>
        {
            var configOptions = new ConfigurationOptions();
            configOptions.EndPoints.Add(_serverAddress);
            configOptions.ClientName = "RedisConnection";
            configOptions.ConnectTimeout = 100000;
            configOptions.SyncTimeout = 100000;
            configOptions.AbortOnConnectFail = false;
            configOptions.Password = _password;
            return configOptions;
        });
        private static readonly object Locker = new object();

        private static Lazy<ConnectionMultiplexer> _conn;

        public IDatabase RedisServer => _conn.Value.GetDatabase();
        public ConnectionMultiplexer Multiplexer => _conn.Value;


    }
}
