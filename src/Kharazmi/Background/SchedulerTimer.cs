using System;
using System.Threading;
using Kharazmi.Helpers;

namespace Kharazmi.Background
{
    public sealed class SchedulerTimer : ISchedulerTimer
    {
        private readonly object _sync = new();

        private Timer? _threadTimer;
        public bool IsRunning { get; private set; }

        public Action<object?>? OnCallback { set; get; }

        public void Start(TimeSpan? delayOnFirstStart = null, TimeSpan? interval = null)
        {
            lock (_sync)
            {
                if (_threadTimer is not null)
                    return;

                var firstTime = delayOnFirstStart ?? TimeSpan.Zero;
                var delay = interval ?? TimeSpan.FromMilliseconds(-1);
                _threadTimer = new Timer(ExecuteJob, DateTimeHelper.DateTimeOffsetUtcNow, firstTime, TimeSpan.FromMilliseconds(-1));
                _threadTimer.Change(delay, TimeSpan.FromMilliseconds(-1));
                IsRunning = true;
            }
        }

        public void Stop()
        {
            lock (_sync)
            {
                if (_threadTimer == null)
                {
                    return;
                }

                _threadTimer.Dispose();
                _threadTimer = null;
                IsRunning = false;
            }
        }

        private void ExecuteJob(object? state)
            => OnCallback?.Invoke(state);
    }
}