using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace GHPCommerce.Infra.OS
{
    public class ThreadPoolAsyncLock
    {
        private readonly ConcurrentQueue<(SemaphoreSlim Semaphore, DateTime Time)> _queue = new ConcurrentQueue<(SemaphoreSlim, DateTime)>();

        private (DateTime start, int requests) _requestTime = (DateTime.UtcNow, 0);

        private readonly SemaphoreSlim _semaphoreSlims;

        public static TimeSpan Timeout = TimeSpan.FromMilliseconds(1000);

        public static int MaxRequest = 1;

        public ThreadPoolAsyncLock(int maxThread = 1)
        {
            _semaphoreSlims = new SemaphoreSlim(maxThread, maxThread);
        }

        public async Task<TaskRunner<TResult>> ScheduleTask<TResult>()
        {
            /* This lock will make sure no one enter in main semaphore unless some one from main semaphore releases this */
            await AcquireOrQueue().WaitAsync(Timeout).ConfigureAwait(false);

            /* Main semaphore wait */
            if (await _semaphoreSlims.WaitAsync(Timeout).ConfigureAwait(false))
            {
               return new TaskRunner<TResult>(ReleaseLock) { IsLocked = true };
            }


            return new TaskRunner<TResult>(ReleaseLock);
        }

        private SemaphoreSlim AcquireOrQueue()
        {
            SemaphoreSlim slim = null;
            lock (_semaphoreSlims)
            {
                if (_semaphoreSlims.CurrentCount > 0 && (DateTime.UtcNow.Subtract(_requestTime.start).Seconds > 1 || _requestTime.requests < MaxRequest))
                {
                    slim = new SemaphoreSlim(1, 1);
                    Interlocked.Increment(ref _requestTime.requests);
                }
                else
                {
                    slim = new SemaphoreSlim(0, 1);
                    _queue.Enqueue((slim, DateTime.UtcNow));
                }
            }

            return slim;
        }

        /// <summary>
        /// Dequeue the thread waiting to enter main semaphore
        /// </summary>
        /// <remarks>
        /// It only allows the dequeue if the main semaphore is not full Or Max requests hasn't been reached within last second.
        /// </remarks>
        private void DeQueue()
        {
            lock (_semaphoreSlims)
            {
                if (_semaphoreSlims.CurrentCount == 0 || (_requestTime.requests >= MaxRequest && DateTime.UtcNow.Subtract(_requestTime.start).Seconds <= 1))
                    return;

                Interlocked.Decrement(ref _requestTime.requests);
                if (_queue.TryDequeue(out var semaphore))
                {
                    semaphore.Semaphore.Release();
                    _requestTime.start = semaphore.Time;
                }
            }
        }

        private void ReleaseLock()
        {
            lock (_semaphoreSlims)
            {
                _semaphoreSlims.Release();
                DeQueue();
            }
        }

        /// <summary>
        /// The disposable releaser tasked with releasing the semaphore.
        /// </summary>
        public sealed class TaskRunner<TResult> : IDisposable
        {
            /// <summary>
            /// A value indicating whether this instance of the given entity has been disposed.
            /// </summary>
            /// <value><see langword="true"/> if this instance has been disposed; otherwise, <see langword="false"/>.</value>
            /// <remarks>
            /// If the entity is disposed, it must not be disposed a second
            /// time. The isDisposed field is set the first time the entity
            /// is disposed. If the isDisposed field is true, then the Dispose()
            /// method will not dispose again. This help not to prolong the entity's
            /// life in the Garbage Collector.
            /// </remarks>
            private bool isDisposed;


            public bool IsLocked { get; set; }

            /// <summary>
            /// A Task to run after acquiring and locking the thread.
            /// </summary>
            /// <remarks>
            /// If the entity acquires the lock, then entity shoule call <see cref="RunAsycn" /> or <see cref="Run"/> to run the task />
            /// </remarks>
            private Task<TResult> task;

            public delegate void TaskDisposeCallBack();

            /// <summary>
            /// Task dispose call back to release or dispose any resources requried for this runner.
            /// </summary>
            private TaskDisposeCallBack taskDisposeCallBack;

            public TaskRunner(TaskDisposeCallBack callBack)
            {
                taskDisposeCallBack = callBack;
            }

            /// <summary>
            /// Finalizes an instance of the <see cref="TaskRunner{TResult}"/> class.
            /// </summary>
            ~TaskRunner()
            {
                // Do not re-create Dispose clean-up code here.
                // Calling Dispose(false) is optimal in terms of
                // readability and maintainability.
                this.Dispose(false);
            }

            /// <summary>
            /// Disposes of the resources (other than memory) used by the module that implements <see cref="T:System.Web.IHttpModule"/>.
            /// </summary>
            public void Dispose()
            {
                this.Dispose(true);

                // This object will be cleaned up by the Dispose method.
                // Therefore, you should call GC.SuppressFinalize to
                // take this object off the finalization queue
                // and prevent finalization code for this object
                // from executing a second time.
                GC.SuppressFinalize(this);
            }

            /// <summary>
            /// Disposes the object and frees resources for the Garbage Collector.
            /// </summary>
            /// <param name="disposing">
            /// If true, the object gets disposed.
            /// </param>
            private void Dispose(bool disposing)
            {
                if (this.isDisposed)
                {
                    return;
                }

                if (disposing)
                {
                    taskDisposeCallBack?.Invoke();
                }

                // Call the appropriate methods to clean up
                // unmanaged resources here.
                // Note disposing is done.
                this.isDisposed = true;
            }
        }
    }
}
