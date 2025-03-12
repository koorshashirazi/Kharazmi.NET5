using System;
using System.Threading.Tasks;
using Kharazmi.Background;
using Kharazmi.Dependency;

namespace Kharazmi.Hangfire.Default
{
    internal class NullBackgroundService : IBackgroundService, INullInstance
    {
        public Task<IJobId> EnqueueJobAsync(IAsyncJob job, JobPriority priority = JobPriority.Normal)
            => Task.FromResult<IJobId>(JobId.New);

        public Task<IJobId> EnqueueJobAsync(IAsyncJob job, IJobId parentJobId, JobPriority priority = JobPriority.Normal)
            => Task.FromResult<IJobId>(JobId.New);

        public IJobId EnqueueJob(IAsyncJob job, JobPriority priority = JobPriority.Normal)
            => JobId.New;

        public Task<IJobId> ScheduleJobAsync(IAsyncJob job, TimeSpan delay = default,
            JobPriority priority = JobPriority.Normal)
            => Task.FromResult<IJobId>(JobId.New);

        public IJobId ScheduleJob(IAsyncJob job, TimeSpan delay = default, JobPriority priority = JobPriority.Normal)
            => JobId.New;

        public Task<bool> RequeueJobAsync(string jobId, string fromState)
            => Task.FromResult(false);

        public bool RequeueJob(string jobId, string fromState)
            => false;

        public Task<bool> DequeueJobAsync(string jobId)
            => Task.FromResult(false);

        public bool DequeueJob(string jobId)
            => false;
    }
}