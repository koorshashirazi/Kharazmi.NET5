#region

using System;
using System.Threading.Tasks;
using Hangfire;
using Kharazmi.Background;
using Kharazmi.Extensions;
using Kharazmi.Threading;
using MongoDB.Driver.Core.Misc;

#endregion

namespace Kharazmi.Hangfire
{
    /// <summary> </summary>
    internal sealed class HangfireBackgroundService : IBackgroundService
    {
        private readonly IBackgroundJobClient _jobClient;

        /// <summary> </summary>
        public HangfireBackgroundService(IBackgroundJobClient jobClient)
        {
            _jobClient = Ensure.IsNotNull(jobClient, nameof(jobClient));
        }


        /// <summary> </summary>
        public Task<IJobId> EnqueueJobAsync(IAsyncJob job,
            JobPriority priority = JobPriority.Normal)
            => ExecuteAsync(job, (c, j) => c.Enqueue<IAsyncJobHandler>(h => h.HandlerAsync(j)));

        public Task<IJobId> EnqueueJobAsync(IAsyncJob job, IJobId parentJobId,
            JobPriority priority = JobPriority.Normal)
            => ExecuteAsync(job, (c, j) => c.ContinueJobWith<IAsyncJobHandler>(parentJobId.Value, h => h.HandlerAsync(j)));

        /// <summary> </summary>
        public IJobId EnqueueJob(IAsyncJob job,
            JobPriority priority = JobPriority.Normal)
            => AsyncHelper.RunSync(() => EnqueueJobAsync(job, priority));


        /// <summary> </summary>
        public Task<IJobId> ScheduleJobAsync(IAsyncJob job, TimeSpan delay = default,
            JobPriority priority = JobPriority.Normal)
            => ExecuteAsync(job, (c, j) => c.Schedule<IAsyncJobHandler>(h => h.HandlerAsync(j), delay));

        /// <summary> </summary>
        public IJobId ScheduleJob(IAsyncJob job, TimeSpan delay = default,
            JobPriority priority = JobPriority.Normal)
            => AsyncHelper.RunSync(() => ScheduleJobAsync(job, delay, priority));

        /// <summary> </summary>
        public Task<bool> RequeueJobAsync(string jobId, string fromState)
            => jobId.IsEmpty() ? Task.FromResult(false) : Task.FromResult(_jobClient.Requeue(jobId, fromState));

        /// <summary> </summary>
        public bool RequeueJob(string jobId, string fromState)
            => AsyncHelper.RunSync(() => RequeueJobAsync(jobId, fromState));

        /// <summary> </summary>
        public Task<bool> DequeueJobAsync(string jobId)
            => jobId.IsEmpty() ? Task.FromResult(false) : Task.FromResult(_jobClient.Delete(jobId));

        /// <summary> </summary>
        public bool DequeueJob(string jobId)
            => AsyncHelper.RunSync(() => DequeueJobAsync(jobId));


        private Task<IJobId> ExecuteAsync(IAsyncJob job,
            Func<IBackgroundJobClient, IAsyncJob, string> jobHandler)
        {
            return Task.FromResult<IJobId>(new JobId(jobHandler(_jobClient, job)));
        }
    }
}