using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using Kharazmi.Constants;

namespace Kharazmi.Options.Hangfire
{
    public class HangfireServerOption : NestedOption
    {
        private readonly HashSet<string> _queues;
        private TimeSpan _stopTimeout;
        private TimeSpan _shutdownTimeout;
        private TimeSpan _schedulePollingInterval;
        private TimeSpan _heartbeatInterval;
        private TimeSpan _serverCheckInterval;
        private TimeSpan _serverTimeout;
        private TimeSpan _cancellationCheckInterval;
        private int _workerCount;

        public HangfireServerOption()
        {
            _workerCount = Math.Min(Environment.ProcessorCount * 5, 20);
            _queues = new HashSet<string> {"default"};
            _stopTimeout = TimeSpan.Zero;
            _shutdownTimeout = TimeSpan.FromSeconds(15.0);
            _schedulePollingInterval = TimeSpan.FromSeconds(15.0);
            _heartbeatInterval = TimeSpan.FromSeconds(30.0);
            _serverTimeout = TimeSpan.FromMinutes(5.0);
            _serverCheckInterval = TimeSpan.FromMinutes(5.0);
            _cancellationCheckInterval = TimeSpan.FromSeconds(5.0);
        }

        /// <summary> </summary>
        [StringLength(100)]
        public string? ServerName { get; set; }

        /// <summary> </summary>
        public int WorkerCount
        {
            get => _workerCount;
            set
            {
                if (value > 0)
                    _workerCount = value;
            }
        }

        /// <summary> </summary>
        public string[]? Queues
        {
            get => _queues.ToArray();
            set
            {
                if (value is null || value.Length <= 0) return;
                foreach (var val in value)
                    _queues.Add(val);
            }
        }

        /// <summary> </summary>
        public TimeSpan StopTimeout
        {
            get => _stopTimeout;
            set
            {
                if (value > TimeSpan.Zero && value.TotalMilliseconds <= int.MaxValue)
                    _stopTimeout = value;
            }
        }

        /// <summary> </summary>
        public TimeSpan ShutdownTimeout
        {
            get => _shutdownTimeout;
            set
            {
                if (value > TimeSpan.Zero && value.TotalMilliseconds <= int.MaxValue)
                    _shutdownTimeout = value;
            }
        }

        /// <summary> </summary>
        public TimeSpan SchedulePollingInterval
        {
            get => _schedulePollingInterval;
            set
            {
                if (value > TimeSpan.Zero && value.TotalMilliseconds <= int.MaxValue)
                    _schedulePollingInterval = value;
            }
        }

        /// <summary> </summary>
        public TimeSpan HeartbeatInterval
        {
            get => _heartbeatInterval;
            set
            {
                if (value > TimeSpan.Zero && value <= TimeSpan.FromHours(24.0))
                    _heartbeatInterval = value;
            }
        }

        /// <summary> </summary>
        public TimeSpan ServerCheckInterval
        {
            get => _serverCheckInterval;
            set
            {
                if (value > TimeSpan.Zero && value <= TimeSpan.FromHours(24.0))
                    _serverCheckInterval = value;
            }
        }

        /// <summary> </summary>
        public TimeSpan ServerTimeout
        {
            get => _serverTimeout;
            set
            {
                if (value > TimeSpan.Zero && value <= TimeSpan.FromHours(24.0))
                    _serverTimeout = value;
            }
        }

        /// <summary> </summary>
        public TimeSpan CancellationCheckInterval
        {
            get => _cancellationCheckInterval;
            set
            {
                if (value > TimeSpan.Zero && value.TotalMilliseconds <= int.MaxValue)
                    _cancellationCheckInterval = value;
            }
        }


        public override void Validate()
        {
            if (_workerCount < 0)
                AddValidation(MessageHelper.MustBeGreaterThan(MessageEventName.OptionsValidation,
                    nameof(HangfireServerOption), nameof(WorkerCount), _workerCount, 0));

            if (_stopTimeout < TimeSpan.Zero &&
                _stopTimeout != Timeout.InfiniteTimeSpan || _stopTimeout.TotalMilliseconds > int.MaxValue)
                AddValidation(MessageHelper.MaxAllowed(MessageEventName.OptionsValidation,
                    nameof(HangfireServerOption), nameof(StopTimeout), _stopTimeout));

            if (_shutdownTimeout < TimeSpan.Zero && _shutdownTimeout != Timeout.InfiniteTimeSpan ||
                _shutdownTimeout.TotalMilliseconds > int.MaxValue)
                AddValidation(MessageHelper.MaxAllowed(MessageEventName.OptionsValidation,
                    nameof(HangfireServerOption), nameof(ShutdownTimeout), _shutdownTimeout));

            if (_schedulePollingInterval < TimeSpan.Zero && _schedulePollingInterval != Timeout.InfiniteTimeSpan ||
                _schedulePollingInterval.TotalMilliseconds > int.MaxValue)
                AddValidation(MessageHelper.MaxAllowed(MessageEventName.OptionsValidation,
                    nameof(HangfireServerOption), nameof(SchedulePollingInterval), _schedulePollingInterval));

            if (_heartbeatInterval < TimeSpan.Zero || _heartbeatInterval > TimeSpan.FromHours(24.0))
                AddValidation(MessageHelper.MaxAllowed(MessageEventName.OptionsValidation,
                    nameof(HangfireServerOption), nameof(HeartbeatInterval), _heartbeatInterval,
                    TimeSpan.FromHours(24.0)));

            if (_serverCheckInterval < TimeSpan.Zero || _serverCheckInterval > TimeSpan.FromHours(24.0))
                AddValidation(MessageHelper.MaxAllowed(MessageEventName.OptionsValidation,
                    nameof(HangfireServerOption), nameof(ServerCheckInterval),
                    _serverCheckInterval, TimeSpan.FromHours(24.0)));

            if (_serverTimeout < TimeSpan.Zero || _serverTimeout > TimeSpan.FromHours(24.0))
                AddValidation(MessageHelper.MaxAllowed(MessageEventName.OptionsValidation,
                    nameof(HangfireServerOption), nameof(ServerTimeout), _serverTimeout,
                    TimeSpan.FromHours(24.0)));

            if (_cancellationCheckInterval < TimeSpan.Zero && _cancellationCheckInterval != Timeout.InfiniteTimeSpan ||
                _cancellationCheckInterval.TotalMilliseconds > int.MaxValue)
                AddValidation(MessageHelper.MaxAllowed(MessageEventName.OptionsValidation,
                    nameof(HangfireServerOption), nameof(CancellationCheckInterval), _cancellationCheckInterval));
        }
    }
}