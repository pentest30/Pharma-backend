using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GHPCommerce.CrossCuttingConcerns.Caching
{
    public interface ICache
    {
        void AddOrUpdate<T>(string key, object value);

        /// <summary>
        /// adds or updates
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task AddOrUpdateAsync<T>(string key, object value, CancellationToken token =default);
        /// <summary>
        /// locks DB using  the transaction during the update
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="token"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task AddOrUpdateWithTransAsync<T>(string key, object value, CancellationToken token = default);

        /// <summary>
        /// adds or updates with expiration time 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expireDateTime"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task AddOrUpdateAsync<T>(string key, object value, DateTimeOffset expireDateTime, CancellationToken token = default);
        /// <summary>
        /// gets an object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keyName"></param>
        /// <returns></returns>
        T Get<T>(string keyName);

        /// <summary>
        /// gets asynchronously an object 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keyName"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<T> GetAsync<T>(string keyName, CancellationToken token = default);
        Task<T> GetWithTransAsync<T>(string keyName, CancellationToken token = default);

        /// <summary>
        /// removes an object by it's id
        /// </summary>
        /// <param name="keyName"></param>
        void Expire<T>(string keyName);

        /// <summary>
        /// removes asynchronously  an object by it's id
        /// </summary>
        /// <param name="keyName"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task ExpireAsync<T>(string keyName, CancellationToken token = default);
        Task ExpireWithTransAsync<T>(string keyName, CancellationToken token = default);
        /// <summary>
        /// get 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keyNames"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<IEnumerable<T>> GetAsync<T>(string[] keyNames, CancellationToken token = default);
    }
}