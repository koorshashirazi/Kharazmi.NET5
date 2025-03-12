#region

using System;
using System.Threading.Tasks;
using Kharazmi.Dependency;

#endregion

namespace Kharazmi.Background
{
    /// <summary>
    /// Job service
    /// </summary>
    public interface IBackgroundService : IShouldBeSingleton, IMustBeInstance
    {
        /// <summary>
        ///  Add a async job to queue 
        /// </summary>
        /// <param name="job"></param>
        /// <param name="priority"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<IJobId> EnqueueJobAsync(IAsyncJob job, JobPriority priority = JobPriority.Normal);


        /// <summary>
        /// Continue job with parent job id
        /// </summary>
        /// <param name="job"></param>
        /// <param name="jobId"></param>
        /// <param name="priority"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<IJobId> EnqueueJobAsync(IAsyncJob job, IJobId jobId, JobPriority priority = JobPriority.Normal);

        /// <summary>
        ///  Add a job to queue 
        /// </summary>
        /// <param name="job"></param>
        /// <param name="priority"></param>
        /// <returns></returns>
        IJobId EnqueueJob(IAsyncJob job, JobPriority priority = JobPriority.Normal);

        /// <summary>
        /// Schedule Job
        /// </summary>
        /// <param name="priority"></param>
        /// <param name="job"></param>
        /// <param name="delay"></param>
        /// <param name="token"></param>
        /// <returns>Return job id</returns>
        Task<IJobId> ScheduleJobAsync(IAsyncJob job, TimeSpan delay = default,
            JobPriority priority = JobPriority.Normal);

        /// <summary>
        /// Schedule Job
        /// </summary>
        /// <param name="priority"></param>
        /// <param name="job"></param>
        /// <param name="delay"></param>
        /// <returns>Return job id</returns>
        IJobId ScheduleJob(IAsyncJob job, TimeSpan delay = default,
            JobPriority priority = JobPriority.Normal);

        /// <summary>
        /// Try to execute an unsuccessful job
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="fromState"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<bool> RequeueJobAsync(string jobId, string fromState);

        /// <summary>
        /// Try to execute an unsuccessful job
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="fromState"></param>
        /// <returns></returns>
        bool RequeueJob(string jobId, string fromState);

        Task<bool> DequeueJobAsync(string jobId);

        bool DequeueJob(string jobId);
    }
}