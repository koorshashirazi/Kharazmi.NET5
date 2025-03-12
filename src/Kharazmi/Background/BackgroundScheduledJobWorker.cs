using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Kharazmi.Background
{
    internal sealed class BackgroundScheduledJobWorker : Scheduler
    {
        private readonly IBackgroundJob _job;

        public BackgroundScheduledJobWorker(IServiceProvider services, IBackgroundJob job) : base(services)
        {
            _job = job;
            SetFirstRun(TimeSpan.FromSeconds(1));
            SetDelay(TimeSpan.FromSeconds(1));
        }

        protected override Task TryExecuteJobAsync(IServiceProvider sp, DateTime now,
            CancellationToken stoppingToken)
        {
            var jobs = _job.GetScheduledJob().Where(c => c.RunJobWhen(now, Started));
            var source = Partitioner.Create(jobs);

            Parallel.ForEach(source, context =>
            {
                ResetTo(now);
                context.InvokeMethod.Invoke(stoppingToken);
            });

            return Task.CompletedTask;
        }
    }
}