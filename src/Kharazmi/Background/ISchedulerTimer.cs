using System;

namespace Kharazmi.Background
{
    public interface ISchedulerTimer
    {
        bool IsRunning { get; }
        Action<object?>? OnCallback { set; get; }
        void Start(TimeSpan? delayOnFirstStart = null, TimeSpan? interval = null);
        void Stop();
    }
}