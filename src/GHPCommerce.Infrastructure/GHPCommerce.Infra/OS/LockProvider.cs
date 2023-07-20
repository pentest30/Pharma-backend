using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

//https://www.ryadel.com/en/asp-net-core-lock-threads-async-custom-ids-lockprovider/
namespace GHPCommerce.Infra.OS
{
    /// <summary>
    /// A LockProvider based upon the SemaphoreSlim class 
    /// to selectively lock objects, resources or statement blocks 
    /// according to given unique IDs in a sync or async way.
    /// </summary>
    public static class LockProvider< TKey> 
    {
        private static readonly ConcurrentDictionary<TKey, SemaphoreSlim> LockDictionary =
            new ConcurrentDictionary<TKey, SemaphoreSlim>();
        private static readonly ConcurrentDictionary<TKey, Semaphore> StreamLock = new ConcurrentDictionary<TKey, Semaphore>();

        public static void GetSemaphore(TKey id)
        {
            StreamLock.GetOrAdd(id, new Semaphore(1, 1)).WaitOne();
        }

        public static void ReleaseSemaphore(TKey id)
        {
            StreamLock.GetOrAdd(id, new Semaphore(1, 1)).Release();
        }
        /// <summary>
        /// Blocks the current thread (according to the given ID)
        /// until it can enter the LockProvider
        /// </summary>
        /// <param name="idToLock">the unique ID to perform the lock</param>
        public static void Wait(TKey idToLock)
        {
            LockDictionary.GetOrAdd(idToLock, new SemaphoreSlim(1, 1)).Wait();
        }

        /// <summary>
        /// Asynchronously puts thread to wait (according to the given ID)
        /// until it can enter the LockProvider
        /// </summary>
        /// <param name="idToLock">the unique ID to perform the lock</param>
        /// <param name="cancellationToken"></param>
        //[Obsolete("Do not use this Function")]
        public static async Task WaitAsync(TKey idToLock,CancellationToken cancellationToken =default)
        {
            await LockDictionary.GetOrAdd(idToLock, new SemaphoreSlim(1, 1)).WaitAsync(cancellationToken);
        }

        /// <summary>
        /// Releases the lock (according to the given ID)
        /// </summary>
        /// <param name="idToUnlock">the unique ID to unlock</param>
        public static void Release(TKey idToUnlock)
        {
            LockDictionary.GetOrAdd(idToUnlock, new SemaphoreSlim(1, 1)).Release();
        }
        public static SemaphoreSlim ProvideLockObject(TKey reportId)
        {
            return LockDictionary.GetOrAdd(reportId, new SemaphoreSlim(1, 1));
        }
    }
}