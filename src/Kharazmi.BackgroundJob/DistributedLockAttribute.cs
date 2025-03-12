using System;
using System.Security.Cryptography;
using System.Threading;
using Hangfire.Common;
using Hangfire.Server;

namespace Kharazmi.Hangfire
{
    internal class DistributedLockAttribute : JobFilterAttribute, IServerFilter
    {
        private readonly TimeSpan _timeout;
        private const string DistributedLock = "DistributedLock";

        public DistributedLockAttribute(TimeSpan timeOut)
        {
            if (timeOut == TimeSpan.Zero || timeOut == Timeout.InfiniteTimeSpan)
                throw new ArgumentException("Timeout argument value should be greater that zero.");
            _timeout = timeOut;
        }

        public void OnPerforming(PerformingContext context)
        {
            try
            {
                var resource = BuildResourceKey(context.BackgroundJob.Job);
                var distributedLock = context.Connection.AcquireDistributedLock(resource, _timeout);
                context.Items[DistributedLock] = distributedLock;
            }
            catch
            {
                context.Canceled = true;
            }
        }

        public void OnPerformed(PerformedContext context)
        {
            if (!context.Items.ContainsKey(DistributedLock))
                throw new InvalidOperationException("Can not release a distributed lock: it was not acquired.");

            var distributedLock = context.Items[DistributedLock] as IDisposable;
            distributedLock?.Dispose();
        }

        private static string BuildResourceKey(Job job)
        {
            var parameters = string.Empty;
            var payload = $"{job.Type.FullName}.{job.Method.Name}.{parameters}";
            var hash = SHA256.Create().ComputeHash(System.Text.Encoding.UTF8.GetBytes(payload));
            var fingerprint = Convert.ToBase64String(hash);
            return fingerprint;
        }
    }
}