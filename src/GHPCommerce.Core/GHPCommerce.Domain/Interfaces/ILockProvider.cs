using System.Threading;
using System.Threading.Tasks;

namespace GHPCommerce.Domain.Interfaces
{
    public interface ILockProvider<in TKey> 
    {
        /// <summary>
        /// Blocks the current thread (according to the given ID)
        /// until it can enter the LockProvider
        /// </summary>
        /// <param name="idToLock">the unique ID to perform the lock</param>
        void Wait(TKey idToLock);

        /// <summary>
        /// Asynchronously puts thread to wait (according to the given ID)
        /// until it can enter the LockProvider
        /// </summary>
        /// <param name="idToLock">the unique ID to perform the lock</param>
        /// <param name="cancellationToken"></param>
        Task WaitAsync(TKey idToLock,CancellationToken cancellationToken =default);

        /// <summary>
        /// Releases the lock (according to the given ID)
        /// </summary>
        /// <param name="idToUnlock">the unique ID to unlock</param>
        void Release(TKey idToUnlock);
    }
}