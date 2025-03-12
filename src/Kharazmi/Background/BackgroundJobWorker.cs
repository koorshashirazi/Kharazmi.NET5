using System;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Constants;
using Microsoft.Extensions.Logging;

namespace Kharazmi.Background
{
    internal sealed class BackgroundJobWorker : Scheduler
    {
        private readonly IBackgroundJob _job;

        public BackgroundJobWorker(IServiceProvider sp, IBackgroundJob job) : base(sp)
            => _job = job;

        protected override async Task TryExecuteJobAsync(IServiceProvider sp, DateTime now,
            CancellationToken stoppingToken)
        {
            var context = await _job.ReleaseAsync(now, stoppingToken);

            if (context.RunJobWhen(now, Started))
            {
                await context.InvokeMethod.Invoke(stoppingToken);
                Logger.LogTrace(MessageTemplate.JobExecuted, MessageEventName.BackgroundWorker,
                    nameof(BackgroundJobWorker), context.JobId, context.JobTypeFullName);
            }
        }
    }
}