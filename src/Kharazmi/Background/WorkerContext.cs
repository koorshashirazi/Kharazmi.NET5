using System;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Constants;
using Kharazmi.Guard;
using Newtonsoft.Json;

namespace Kharazmi.Background
{
    public class WorkerContext
    {
        public WorkerContext(Func<CancellationToken, Task> task, JobId jobId, Type jobType, TimeSpan? delay = null)
        {
            InvokeMethod = task.NotNull(nameof(task));
            JobId = jobId;
            JobType = jobType;
            JobTypeFullName = jobType.FullName ?? jobType.Name;
            Delay = delay;
            OccurredOn = DateTimeConstants.UtcNow;
            RunJobWhen = now => now <= (Delay.HasValue ? now.Add(Delay.Value) : DateTimeOffset.UtcNow);
        }


        public DateTimeOffset OccurredOn { get; }
        [JsonIgnore] public Type JobType { get; }
        public string JobTypeFullName { get; set; }
        public JobId JobId { get; }
        public TimeSpan? Delay { get; }
        public Func<DateTimeOffset, bool> RunJobWhen { get; }
        public Func<CancellationToken, Task> InvokeMethod { get; }
    }
}