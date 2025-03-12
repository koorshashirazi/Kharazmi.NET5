using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Kharazmi.Background
{
    public interface IBackgroundJob
    {
        IEnumerable<BackgroundJobContext> GetScheduledJob();

        IJobId Enqueue(Func<CancellationToken, Task> invokeMethod, Type jobType, TimeSpan? delay = null);

        Task<IJobId> EnqueueAsync(Func<CancellationToken, Task> invokeMethod, Type jobType,
            TimeSpan? delay = null);

        IJobId Enqueue(BackgroundJobContext backgroundJobContext);
        Task<IJobId> EnqueueAsync(BackgroundJobContext backgroundJobContext);

        bool Requeue(string jobId);
        bool Dequeue(string jobId);
        void AddFailed(BackgroundJobContext? workerContext);
        ValueTask<BackgroundJobContext> ReleaseAsync(DateTime now, CancellationToken token = default);
    }
}