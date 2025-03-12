using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using Kharazmi.Constants;
using Kharazmi.Extensions;
using Kharazmi.HealthChecks;
using Kharazmi.Options.HealthCheck;

namespace Kharazmi.Options.Hangfire
{
    /// <summary>_</summary>
    public class HangfireMongoStorageOption : NestedOption, IHaveHealthCheckOption
    {
        private TimeSpan _queuePollInterval;
        private TimeSpan _invisibilityTimeout;
        private TimeSpan _distributedLockLifetime;
        private TimeSpan _migrationLockTimeout;
        private TimeSpan _connectionCheckTimeout;
        private TimeSpan _countersAggregateInterval;
        private TimeSpan _jobExpirationCheckInterval;

        /// <summary>_</summary>
        public HangfireMongoStorageOption()
        {
            Prefix = "hangfire";
            _queuePollInterval = TimeSpan.FromSeconds(15.0);
            _invisibilityTimeout = new TimeSpan();
            _distributedLockLifetime = TimeSpan.FromSeconds(30.0);
            _jobExpirationCheckInterval = TimeSpan.FromHours(1.0);
            _countersAggregateInterval = TimeSpan.FromMinutes(5.0);
            _migrationLockTimeout = TimeSpan.FromMinutes(1.0);
            _connectionCheckTimeout = TimeSpan.FromSeconds(5.0);
            CheckConnection = true;
            ConnectionString = "mongodb://localhost:27017/hangfire";
        }

        /// <summary>_</summary>
        [StringLength(20)]
        public string Prefix { get; set; }

        /// <summary>_</summary>
        public string ConnectionString { get; set; }

        /// <summary>_</summary>
        public TimeSpan QueuePollInterval
        {
            get => _queuePollInterval;
            set
            {
                if (value > TimeSpan.Zero && value.TotalMilliseconds <= int.MaxValue)
                    _queuePollInterval = value;
            }
        }

        /// <summary>_</summary>
        public TimeSpan InvisibilityTimeout
        {
            get => _invisibilityTimeout;
            set
            {
                if (value >= TimeSpan.Zero && value.TotalMilliseconds <= int.MaxValue)
                    _invisibilityTimeout = value;
            }
        }

        /// <summary>_</summary>
        public TimeSpan DistributedLockLifetime
        {
            get => _distributedLockLifetime;
            set
            {
                if (value > TimeSpan.Zero && value.TotalMilliseconds <= int.MaxValue)
                    if (!(value != value.Duration()))
                        _distributedLockLifetime = value;
            }
        }

        /// <summary>_</summary>
        public TimeSpan MigrationLockTimeout
        {
            get => _migrationLockTimeout;
            set
            {
                if (value > TimeSpan.Zero && value.TotalMilliseconds <= int.MaxValue)
                    if (!(value != value.Duration()))
                        _migrationLockTimeout = value;
            }
        }

        /// <summary>_</summary>
        public bool CheckConnection { get; set; }

        /// <summary>_</summary>
        public TimeSpan ConnectionCheckTimeout
        {
            get => _connectionCheckTimeout;
            set
            {
                if (value > TimeSpan.Zero && value.TotalMilliseconds <= int.MaxValue)
                    _connectionCheckTimeout = value;
            }
        }

        /// <summary>_</summary>
        public TimeSpan JobExpirationCheckInterval
        {
            get => _jobExpirationCheckInterval;
            set
            {
                if (value > TimeSpan.Zero && value.TotalMilliseconds <= int.MaxValue)
                    _jobExpirationCheckInterval = value;
            }
        }

        /// <summary>_</summary>
        public TimeSpan CountersAggregateInterval
        {
            get => _countersAggregateInterval;
            set
            {
                if (value > TimeSpan.Zero && value.TotalMilliseconds <= int.MaxValue)
                    _countersAggregateInterval = value;
            }
        }

        public bool UseHealthCheck { get; set; }

        /// <summary>_</summary>
        public HealthCheckOption? HealthCheckOption { get; set; }


        /// <summary>_</summary>
        public override void Validate()
        {
            if (ConnectionString.IsEmpty())
                AddValidation(MessageHelper.NullOrEmpty(MessageEventName.OptionsValidation,
                    nameof(HangfireMongoStorageOption), nameof(ConnectionString)));


            if (Prefix.IsEmpty())
                AddValidation(MessageHelper.NullOrEmpty(MessageEventName.OptionsValidation,
                    nameof(HangfireMongoStorageOption), nameof(Prefix)));

            if (_queuePollInterval <= TimeSpan.Zero &&
                _queuePollInterval != Timeout.InfiniteTimeSpan ||
                _queuePollInterval.TotalMilliseconds > int.MaxValue)
                AddValidation(MessageHelper.MaxAllowed(MessageEventName.OptionsValidation,
                    nameof(HangfireMongoStorageOption), nameof(QueuePollInterval), _queuePollInterval));

            if (_invisibilityTimeout < TimeSpan.Zero &&
                _invisibilityTimeout != Timeout.InfiniteTimeSpan ||
                _invisibilityTimeout.TotalMilliseconds > int.MaxValue)
                AddValidation(MessageHelper.MaxAllowed(MessageEventName.OptionsValidation,
                    nameof(HangfireMongoStorageOption), nameof(InvisibilityTimeout), _invisibilityTimeout));

            if (_distributedLockLifetime <= TimeSpan.Zero &&
                _distributedLockLifetime != Timeout.InfiniteTimeSpan ||
                _distributedLockLifetime.TotalMilliseconds > int.MaxValue)
                AddValidation(MessageHelper.MaxAllowed(MessageEventName.OptionsValidation,
                    nameof(HangfireMongoStorageOption), nameof(DistributedLockLifetime), _distributedLockLifetime));

            if (_migrationLockTimeout <= TimeSpan.Zero &&
                _migrationLockTimeout != Timeout.InfiniteTimeSpan ||
                _migrationLockTimeout.TotalMilliseconds > int.MaxValue)
                AddValidation(MessageHelper.MaxAllowed(MessageEventName.OptionsValidation,
                    nameof(HangfireMongoStorageOption), nameof(MigrationLockTimeout), _migrationLockTimeout));


            if (_connectionCheckTimeout <= TimeSpan.Zero &&
                _connectionCheckTimeout != Timeout.InfiniteTimeSpan ||
                _connectionCheckTimeout.TotalMilliseconds > int.MaxValue)
                AddValidation(MessageHelper.MaxAllowed(MessageEventName.OptionsValidation,
                    nameof(HangfireMongoStorageOption), nameof(ConnectionCheckTimeout), _connectionCheckTimeout));

            if (_jobExpirationCheckInterval <= TimeSpan.Zero &&
                _jobExpirationCheckInterval != Timeout.InfiniteTimeSpan ||
                _jobExpirationCheckInterval.TotalMilliseconds > int.MaxValue)
                AddValidation(MessageHelper.MaxAllowed(MessageEventName.OptionsValidation,
                    nameof(HangfireMongoStorageOption), nameof(JobExpirationCheckInterval),
                    _jobExpirationCheckInterval));

            if (_countersAggregateInterval <= TimeSpan.Zero &&
                _countersAggregateInterval != Timeout.InfiniteTimeSpan ||
                _countersAggregateInterval.TotalMilliseconds > int.MaxValue)
                AddValidation(MessageHelper.MaxAllowed(MessageEventName.OptionsValidation,
                    nameof(HangfireMongoStorageOption), nameof(CountersAggregateInterval),
                    _countersAggregateInterval));

            if (!UseHealthCheck) return;
            HealthCheckOption ??= new HealthCheckOption();
            HealthCheckOption.Validate();
        }
    }
}