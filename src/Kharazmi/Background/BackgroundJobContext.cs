using System;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Guard;
using Kharazmi.Helpers;
using Newtonsoft.Json;

namespace Kharazmi.Background
{
    public class BackgroundJobContext
    {
        public BackgroundJobContext(Func<CancellationToken, Task> task, JobId jobId, Type jobType,
            TimeSpan? delay = null)
        {
            InvokeMethod = task.NotNull(nameof(task));
            JobId = jobId;
            JobType = jobType;
            JobTypeFullName = jobType.FullName ?? jobType.Name;
            Delay = delay;
            OccurredOn = DateTimeHelper.DateTimeUtcNow;
            RunJobWhen = (now, runStartTime) =>
                !Delay.HasValue || now.Ticks - runStartTime.Add(Delay.Value).Ticks >= 0;
        }


        public DateTime OccurredOn { get; }
        [JsonIgnore] public Type JobType { get; }
        public string JobTypeFullName { get; set; }
        public JobId JobId { get; }
        public TimeSpan? Delay { get; }
        public Func<DateTime, DateTime, bool> RunJobWhen { get; }
        public Func<CancellationToken, Task> InvokeMethod { get; }
    }
}