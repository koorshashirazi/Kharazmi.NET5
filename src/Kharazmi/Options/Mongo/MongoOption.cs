using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kharazmi.Constants;
using Kharazmi.Extensions;
using Kharazmi.HealthChecks;
using Kharazmi.Helpers;
using Kharazmi.Options.HealthCheck;

namespace Kharazmi.Options.Mongo
{
    public class MongoOption : ChildOption, IHaveHealthCheckOption
    {
        private TimeSpan _expiredScanInterval;
        private string _migrationStrategy;
        private bool _enableMigration;
        private bool _enableSeed;
        private bool _seedWithReplace;

        public MongoOption()
        {
            Host = "127.0.0.1";
            Port = 27017;
            Localization = DateTimeHelper.CultureInfo.TwoLetterISOLanguageName;
            DatabaseVersion = "0.1.0";
            ThrowIfMigrationFailed = false;
            DocumentTypesAssembly = "";
            _expiredScanInterval = TimeSpan.FromMinutes(3);
            _enableMigration = false;
            _enableSeed = false;
            _seedWithReplace = false;
            _migrationStrategy = DatabaseMigrationStrategy.Ignore;
            Database = $"Database-{Guid.NewGuid():N}";
            ApplicationName = Assembly.GetEntryAssembly()?.FullName ?? $"ApplicationName{Guid.NewGuid():N}";
        }

        public string ApplicationName { get; set; }
        public string? ReplicaSetName { get; set; }
        public string Database { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public string? ConnectionString { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public bool ThrowIfMigrationFailed { get; set; }
        public string Localization { get; set; }
        public string DatabaseVersion { get; set; }
        public string DocumentTypesAssembly { get; set; }

        public string? AssemblyDbInstaller { get; set; }

        public bool EnableMigration
        {
            get => _enableMigration;
            set => _enableMigration = value;
        }

        public string MigrationStrategy
        {
            get => _enableMigration ? _migrationStrategy : DatabaseMigrationStrategy.Ignore;
            set
            {
                if (MigrationStrategies.Contains(value))
                    _migrationStrategy = value;
            }
        }

        public IReadOnlyCollection<string> MigrationStrategies
            => DatabaseMigrationStrategy.GetDatabaseMigrationStrategy;

        public bool EnableSeed
        {
            get => _enableMigration && _enableSeed;
            set => _enableSeed = value;
        }

        public bool SeedWithReplace
        {
            get => _enableSeed && _seedWithReplace;
            set => _seedWithReplace = value;
        }

        public TimeSpan ExpiredScanInterval
        {
            get => _expiredScanInterval;
            set
            {
                if (value <= TimeSpan.Zero || value > TimeSpan.FromDays(1))
                    return;
                _expiredScanInterval = value;
            }
        }

        public bool UseHealthCheck { get; set; }
        public HealthCheckOption? HealthCheckOption { get; set; }

        public override void Validate()
        {
            if (Database.IsEmpty())
                AddValidation(MessageHelper.NullOrEmpty(MessageEventName.OptionsValidation,
                    nameof(MongoOption), nameof(Database)));

            if (ConnectionString.IsEmpty() && Host.IsEmpty())
            {
                AddValidation(MessageHelper.NullOrEmpty(MessageEventName.OptionsValidation,
                    nameof(MongoOption), nameof(ConnectionString)));

                AddValidation(MessageHelper.NullOrEmpty(MessageEventName.OptionsValidation,
                    nameof(MongoOption), nameof(Host)));
            }

            if (EnableMigration && DocumentTypesAssembly.IsEmpty())
                AddValidation(MessageHelper.NullOrEmpty(MessageEventName.OptionsValidation,
                    nameof(MongoOption), nameof(DocumentTypesAssembly)));

            if (!UseHealthCheck) return;
            HealthCheckOption ??= new HealthCheckOption();
            HealthCheckOption.Validate();
        }

        public override string ToString()
            => $"ApplicationName: {ApplicationName}, Database: {Database}";
    }
}