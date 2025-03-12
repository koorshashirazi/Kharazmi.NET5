using System;
using System.Threading;
using System.Threading.Tasks;

namespace Kharazmi.Threading
{
    public sealed class TaskQueue : IDisposable
    {
        private readonly SemaphoreSlim _semaphore;

        public TaskQueue() : this(1)
        { }

        public TaskQueue(int init)
        {
            _semaphore = new SemaphoreSlim(init);
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