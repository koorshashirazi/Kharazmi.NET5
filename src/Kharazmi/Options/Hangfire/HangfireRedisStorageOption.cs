using System;
using Kharazmi.Constants;
using Kharazmi.Extensions;
using Kharazmi.HealthChecks;
using Kharazmi.Options.HealthCheck;

namespace Kharazmi.Options.Hangfire
{
    public class HangfireRedisStorageOption : NestedOption, IHaveHealthCheckOption
    {
        private TimeSpan _invisibilityTimeout;
        private TimeSpan _fetchTimeout;
        private TimeSpan _expiryCheckInterval;
        private int _succeededListSize;
        private int _deletedListSize;

        public HangfireRedisStorageOption()
        {
            _invisibilityTimeout = TimeSpan.FromMinutes(30.0);
            _fetchTimeout = TimeSpan.FromMinutes(3.0);
            _expiryCheckInterval = TimeSpan.FromHours(1.0);
            Db = 0;
            Prefix = "{hangfire}:";
            SucceededListSize = 499;
            DeletedListSize = 499;
            LifoQueues = new string[0];
            UseTransactions = true;
            ConnectionString = "localhost:6379,abortConnect=true,ssl=false";
        }

        /// <summary> </summary>
        public string? ConnectionString { get; set; }

        public string Prefix { get; set; }

        public int Db { get; set; }

        public TimeSpan InvisibilityTimeout
        {
            get => _invisibilityTimeout;
            set
            {
                if (value > TimeSpan.Zero && value.TotalMilliseconds <= int.MaxValue)
                    _invisibilityTimeout = value;
            }
        }

        public TimeSpan FetchTimeout
        {
            get => _fetchTimeout;
            set
            {
                if (value > TimeSpan.Zero && value.TotalMilliseconds <= int.MaxValue)
                    _fetchTimeout = value;
            }
        }

        public TimeSpan ExpiryCheckInterval
        {
            get => _expiryCheckInterval;
            set
            {
                if (value > TimeSpan.Zero && value.TotalMilliseconds <= int.MaxValue)
                    _expiryCheckInterval = value;
            }
        }

        public int SucceededListSize
        {
            get => _succeededListSize;
            set
            {
                if (value > 0)
                    _succeededListSize = value;
            }
        }

        public int DeletedListSize
        {
            get => _deletedListSize;
            set
            {
                if (value > 0)
                    _deletedListSize = value;
            }
        }

        public string[] LifoQueues { get; set; }

        public bool UseTransactions { get; set; }

        public bool UseHealthCheck { get; set; }
        public HealthCheckOption? HealthCheckOption { get; set; }

        public override void Validate()
        {
            if (ConnectionString.IsEmpty())
                AddValidation(MessageHelper.NullOrEmpty(MessageEventName.OptionsValidation,
                    nameof(HangfireRedisStorageOption), nameof(ConnectionString)));
            if (Db < 0)
                AddValidation(MessageHelper.MustBeGreaterThan(MessageEventName.OptionsValidation,
                    nameof(HangfireRedisStorageOption), nameof(Db), Db, 0));

            if (InvisibilityTimeout <= TimeSpan.Zero &&
                InvisibilityTimeout.TotalMilliseconds > int.MaxValue)
                AddValidation(MessageHelper.MustBeBetween(MessageEventName.OptionsValidation,
                    nameof(HangfireRedisStorageOption), nameof(InvisibilityTimeout), _invisibilityTimeout));

            if (FetchTimeout <= TimeSpan.Zero ||
                FetchTimeout.TotalMilliseconds > int.MaxValue)
                AddValidation(MessageHelper.MustBeBetween(MessageEventName.OptionsValidation,
                    nameof(HangfireRedisStorageOption), nameof(FetchTimeout), _fetchTimeout));


            if (ExpiryCheckInterval <= TimeSpan.Zero ||
                ExpiryCheckInterval.TotalMilliseconds > int.MaxValue)
                AddValidation(MessageHelper.MustBeBetween(MessageEventName.OptionsValidation,
                    nameof(HangfireRedisStorageOption), nameof(ExpiryCheckInterval), _expiryCheckInterval));

            if (!UseHealthCheck) return;
            HealthCheckOption ??= new HealthCheckOption();
            HealthCheckOption.Validate();
        }
    }
}