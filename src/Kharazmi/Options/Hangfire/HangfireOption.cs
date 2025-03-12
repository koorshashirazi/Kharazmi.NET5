#region

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Kharazmi.Constants;
using Kharazmi.Extensions;
using Kharazmi.HealthChecks;
using Kharazmi.Options.HealthCheck;

#endregion

namespace Kharazmi.Options.Hangfire
{
    /// <summary> </summary>
    public class HangfireOption : ConfigurePluginOption, IHaveHealthCheckOption, IHaveNestedHealthCheck
    {
        private readonly HashSet<string> _jobStorageTypes;
        private readonly HashSet<string> _loggerProviders;
        private readonly HashSet<int> _delaysInSeconds;
        private bool _useDashboard;
        private bool _useAuthorization;

        /// <summary> </summary>
        public HangfireOption()
        {
            _delaysInSeconds = new HashSet<int> {5, 10, 15};
            MinLogger = 0; // TODO get from config
            HangfireServerOption = new HangfireServerOption();
            HangfireInMemoryStorageOption = new HangfireInMemoryStorageOption();
            JobStorageType = Constants.JobStorageTypes.InMemory;
            _jobStorageTypes = new HashSet<string>
            {
                Constants.JobStorageTypes.InMemory,
                Constants.JobStorageTypes.Redis,
                Constants.JobStorageTypes.MongoDb
            };
            _loggerProviders = new HashSet<string>
            {
                "None",
                "Console",
                "Default",
                "Serilog"
            };
        }


        /// <summary> </summary>
        public bool UseDashboard
        {
            get => Enable && _useDashboard;
            set => _useDashboard = value;
        }

        /// <summary> </summary>
        public bool UseAuthorization
        {
            get => Enable && _useAuthorization;
            set => _useAuthorization = value;
        }

        /// <summary> </summary>
        [StringLength(100)]
        public string? AppPath { get; set; }

        /// <summary> </summary>
        [StringLength(100)]
        public string? PrefixPath { get; set; }

        /// <summary> </summary>
        public int StatsPollingInterval { get; set; }

        /// <summary> </summary>
        public bool DisplayStorageConnectionString { get; set; }

        /// <summary> </summary>
        [StringLength(100)]
        public string? DashboardTitle { get; set; }

        /// <summary> </summary>
        public bool IgnoreAntiforgeryToken { get; set; }

        /// <summary> </summary>
        [StringLength(100)]
        public string? PolicyName { get; set; }

        /// <summary> </summary>
        [StringLength(100)]
        public string? PathMatch { get; set; }

        /// <summary> </summary>
        public int CancellationFromSeconds { get; set; }

        /// <summary> </summary>
        public int Attempts { get; set; } = 3;

        /// <summary> </summary>
        public int[]? DelaysInSeconds
        {
            get => _delaysInSeconds.ToArray();
            set
            {
                if (value is null || value.Length <= 0) return;
                foreach (var val in value)
                    _delaysInSeconds.Add(val);
            }
        }

        /// <summary> </summary>
        public string JobStorageType { get; set; }

        public IReadOnlyCollection<string> JobStorageTypes => _jobStorageTypes;

        /// <summary>_</summary>
        [StringLength(100)]
        public string? LoggerProvider { get; set; }

        public IReadOnlyCollection<string> LoggerProviders => _loggerProviders;

        [Range(0, 5)] public int MinLogger { get; set; }

        /// <summary>_</summary>
        public int? MaximumJobsFailed { get; set; }

        /// <summary>_</summary>
        public int? MinimumAvailableServers { get; set; }

        /// <summary>_</summary>
        public HangfireServerOption HangfireServerOption { get; set; }

        /// <summary>_</summary>
        public HangfireInMemoryStorageOption HangfireInMemoryStorageOption { get; set; }

        /// <summary>_</summary>
        public HangfireRedisStorageOption? HangfireRedisStorageOption { get; set; }

        /// <summary>_</summary>
        public HangfireMongoStorageOption? HangfireMongoStorageOption { get; set; }

        public bool UseHealthCheck { get; set; }

        /// <summary>_</summary>
        public HealthCheckOption? HealthCheckOption { get; set; }


        /// <summary>_</summary>
        public override void Validate()
        {
            if (PathMatch.IsEmpty())
                AddValidation(MessageHelper.NullOrEmpty(MessageEventName.OptionsValidation,
                    nameof(HangfireOption), nameof(PathMatch)));
            HangfireServerOption.Validate();

            switch (JobStorageType)
            {
                case Constants.JobStorageTypes.InMemory:
                    HangfireInMemoryStorageOption.Validate();
                    break;
                case Constants.JobStorageTypes.Redis:
                    HangfireRedisStorageOption ??= new HangfireRedisStorageOption();
                    HangfireRedisStorageOption.Validate();
                    break;
                case Constants.JobStorageTypes.MongoDb:
                    HangfireMongoStorageOption ??= new HangfireMongoStorageOption();
                    HangfireMongoStorageOption.Validate();
                    break;
                default:
                    AddValidation(MessageHelper.HangfireInvalidStorage(MessageEventName.OptionsValidation,
                        nameof(HangfireOption)));
                    break;
            }

            if (!UseHealthCheck) return;
            HealthCheckOption ??= new HealthCheckOption();
            HealthCheckOption.Validate();
        }

        public IEnumerable<INestedOption?> GetNestedOption()
        {
            yield return HangfireRedisStorageOption;
            yield return HangfireMongoStorageOption;
        }
    }
}