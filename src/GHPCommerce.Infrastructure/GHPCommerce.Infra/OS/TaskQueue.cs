using System;
using System.Threading;
using System.Threading.Tasks;

namespace GHPCommerce.Infra.OS
{
    public interface ITaskQueue : IDisposable
    {
        Task<T> Enqueue<T>(Func<Task<T>> taskGenerator, CancellationToken token);
        Task Enqueue(Func<Task> taskGenerator, CancellationToken token);
    }

    public sealed class TaskQueue : ITaskQueue
    {
        private readonly SemaphoreSlim _semaphore;

        public TaskQueue() : this(degreesOfParallelism: 1)
        { }

        public TaskQueue(int degreesOfParallelism)
        {
            _semaphore = new SemaphoreSlim(degreesOfParallelism, degreesOfParallelism);
        }

        public async Task<T> Enqueue<T>(Func<Task<T>> taskGenerator, CancellationToken token)
        {
            await _semaphore.WaitAsync(token);
            try
            {
                return await taskGenerator();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task Enqueue(Func<Task> taskGenerator, CancellationToken token)
        {
            await _semaphore.WaitAsync(token);
            try
            {
                await taskGenerator();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public void Dispose()
        {
            _semaphore.Dispose();
        }
    }
}
