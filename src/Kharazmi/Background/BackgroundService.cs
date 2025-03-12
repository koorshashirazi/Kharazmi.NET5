using System;
using System.Threading.Tasks;
using Kharazmi.Threading;

namespace Kharazmi.Background
{
    internal sealed class BackgroundService : IBackgroundService
    {
        private readonly IBackgroundJob _job;
        private readonly IAsyncJobHandler _jobHandler;

        public BackgroundService(IBackgroundJob job, IAsyncJobHandler jobHandler)
        {
            _job = job;
            _jobHandler = jobHandler;
        }

        public Task<IJobId> EnqueueJobAsync(IAsyncJob job, JobPriority priority = JobPriority.Normal)
            => _job.EnqueueAsync(_ => _jobHandler.HandlerAsync(job), job.GetType());

        public Task<IJobId> EnqueueJobAsync(IAsyncJob job, IJobId parentJobId,
            JobPriority priority = JobPriority.Normal)
        {
            throw new NotImplementedException("TODO, Use hangfire instead");
        }

        public IJobId EnqueueJob(IAsyncJob job, JobPriority priority = JobPriority.Normal)
            => AsyncHelper.RunSync(() => EnqueueJobAsync(job, priority));

        public Task<IJobId> ScheduleJobAsync(IAsyncJob job, TimeSpan delay = default,
            JobPriority priority = JobPriority.Normal)
            => _job.EnqueueAsync(_ => _jobHandler.HandlerAsync(job), job.GetType(), delay);

        public IJobId ScheduleJob(IAsyncJob job, TimeSpan delay = default, JobPriority priority = JobPriority.Normal)
            => AsyncHelper.RunSync(() => ScheduleJobAsync(job, delay, priority));

        public Task<bool> RequeueJobAsync(string jobId, string fromState)
            => Task.FromResult(_job.Requeue(jobId));

        public bool RequeueJob(string jobId, string fromState)
            => AsyncHelper.RunSync(() => RequeueJobAsync(jobId, fromState));

        public Task<bool> DequeueJobAsync(string jobId)
            => Task.FromResult(_job.Dequeue(jobId));

        public bool DequeueJob(string jobId)
            => AsyncHelper.RunSync(() => DequeueJobAsync(jobId));
    }
}