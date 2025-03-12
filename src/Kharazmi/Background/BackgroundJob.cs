using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Kharazmi.Guard;
using Kharazmi.Threading;

namespace Kharazmi.Background
{
    internal class BackgroundJob : IBackgroundJob
    {
        private readonly Channel<BackgroundJobContext> _queue;
        private readonly ConcurrentDictionary<string, BackgroundJobContext> _backgroundJobContexts = new();
        private readonly ConcurrentDictionary<string, BackgroundJobContext> _failedWorkerContexts = new();

        public BackgroundJob()
        {
            var options = new BoundedChannelOptions(100)
            {
                FullMode = BoundedChannelFullMode.Wait
            };
            _queue = Channel.CreateBounded<BackgroundJobContext>(options);
        }


        public IEnumerable<BackgroundJobContext> GetScheduledJob()
            => _backgroundJobContexts.Select(x => x.Value);

        public IJobId Enqueue(Func<CancellationToken, Task> invokeMethod, Type jobType, TimeSpan? delay = null)
            => AsyncHelper.RunSync(() => EnqueueAsync(invokeMethod, jobType, delay));


        public async Task<IJobId> EnqueueAsync(Func<CancellationToken, Task> invokeMethod, Type jobType,
            TimeSpan? delay = null)
        {
            var mutex = new Mutex();
            try
            {
                mutex.WaitOne();
                invokeMethod.NotNull(nameof(invokeMethod));

                var jobId = JobId.New;
                var context = new BackgroundJobContext(invokeMethod, jobId, jobType, delay);

                if (context.Delay.HasValue)
                    _backgroundJobContexts.TryAdd(jobId, context);
                else
                {
                    var result = await _queue.Writer.WaitToWriteAsync();
                    if (result)
                        await _queue.Writer.WriteAsync(context);
                }

                return jobId;
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }

        public IJobId Enqueue(BackgroundJobContext backgroundJobContext)
            => AsyncHelper.RunSync(() => EnqueueAsync(backgroundJobContext));

        public Task<IJobId> EnqueueAsync(BackgroundJobContext backgroundJobContext)
            => EnqueueAsync(backgroundJobContext.InvokeMethod, backgroundJobContext.JobType,
                backgroundJobContext.Delay);

        public bool Requeue(string jobId)
        {
            var result = _backgroundJobContexts.TryGetValue(jobId, out var workerContext);
            if (workerContext is not null)
                Enqueue(workerContext.InvokeMethod, workerContext.JobType, workerContext.Delay);

            return result;
        }

        public bool Dequeue(string jobId)
        {
            var result = _backgroundJobContexts.TryRemove(jobId, out var workerContext);
            if (workerContext is not null)
            {
                // TODO
            }

            return result;
        }

        public void AddFailed(BackgroundJobContext? workerContext)
        {
            if (workerContext is not null)
                _failedWorkerContexts.TryAdd(workerContext.JobId, workerContext);
        }

        public ValueTask<BackgroundJobContext> ReleaseAsync(DateTime now,
            CancellationToken stoppingToken = default)
            => _queue.Reader.ReadAsync(stoppingToken);
    }
}