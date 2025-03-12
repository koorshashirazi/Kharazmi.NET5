using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Kharazmi.Constants;
using Kharazmi.Extensions;
using Kharazmi.HealthChecks;
using Kharazmi.Options.Domain;
using Kharazmi.Options.HealthCheck;

namespace Kharazmi.Options.Redis
{
    /// <summary> </summary>
    public class RedisDbOption : ChildOption, IChannelOptions, IHaveHealthCheckOption
    {
        private readonly HashSet<string> _patternModes;
        private readonly HashSet<string> _commandFlags;
        private readonly HashSet<string> _sslProtocolTypes;

        /// <summary>_</summary>
        public RedisDbOption()
        {
            PublishAsynchronous = true;
            SubscribeAsynchronous = true;
            ThrowExceptionOnSubscribeFailed = true;
            Hosts = Array.Empty<RedisHost>();
            ServerEnumerationStrategy = new ServerEnumerationStrategy();

            SslProtocols = SslProtocolType.None;
            _sslProtocolTypes = new HashSet<string>
            {
                SslProtocolType.None,
                SslProtocolType.Tls,
                SslProtocolType.Tls11,
                SslProtocolType.Tls12,
                SslProtocolType.Tls13
            };
            DefaultPatternMode = PatternMode.Auto;
            _patternModes = new HashSet<string>
            {
                PatternMode.Auto,
                PatternMode.Literal,
                PatternMode.Pattern,
            };
            DefaultCommandFlags = CommandFlag.None;
            _commandFlags = new HashSet<string>
            {
                CommandFlag.None,
                CommandFlag.DemandMaster,
                CommandFlag.DemandReplica,
                CommandFlag.NoRedirect,
                CommandFlag.PreferMaster,
                CommandFlag.PreferReplica,
                CommandFlag.FireAndForget,
                CommandFlag.NoScriptCache,
            };
        }


        /// <summary> </summary>
        [StringLength(100)]
        public string? ChannelPrefix { get; set; }

        /// <summary> </summary>
        public string DefaultPatternMode { get; set; }

        /// <summary> </summary>
        public IReadOnlyCollection<string> PatternModes => _patternModes;

        /// <summary> </summary>
        public string DefaultCommandFlags { get; set; }

        /// <summary> </summary>
        public IReadOnlyCollection<string> CommandFlags => _commandFlags;

        /// <summary> </summary>
        public bool PublishAsynchronous { get; set; }

        /// <summary> </summary>
        public bool SubscribeAsynchronous { get; set; }

        /// <summary> </summary>
        public bool ThrowExceptionOnSubscribeFailed { get; set; }

        /// <summary>_</summary>
        [StringLength(100)]
        public string? ServiceName { get; set; }

        /// <summary>_</summary>
        public string SslProtocols { get; set; }

        /// <summary>_</summary>
        public IReadOnlyCollection<string> SslProtocolTypes => _sslProtocolTypes;

        /// <summary>_</summary>
        [StringLength(100)]
        public string? ConnectionString { get; set; }

        /// <summary>_</summary>
        [StringLength(100)]
        public string? ConfigurationChannel { get; set; }

        /// <summary>_</summary>
        [StringLength(100)]
        public string? KeyPrefix { get; set; }

        /// <summary>_</summary>
        [StringLength(100)]
        public string? Password { get; set; }

        /// <summary>_</summary>
        public bool AllowAdmin { get; set; }

        /// <summary>_</summary>
        public bool Ssl { get; set; }

        /// <summary>_</summary>
        public int ConnectTimeout { get; set; } = 5000;

        /// <summary>_</summary>
        public int SyncTimeout { get; set; } = 1000;

        /// <summary>_</summary>
        public bool AbortOnConnectFail { get; set; }

        /// <summary>_</summary>
        public int Database { get; set; } = 0;

        /// <summary>_</summary>
        public RedisHost[] Hosts { get; set; }

        /// <summary>_</summary>
        public uint MaxValueLength { get; set; }

        /// <summary>_</summary>
        public int PoolSize { get; set; } = 5;

        /// <summary>_</summary>
        public string[]? ExcludeCommands { get; set; }

        /// <summary>_</summary>
        public ServerEnumerationStrategy ServerEnumerationStrategy { get; set; }

        public bool UseRetry { get; set; }
        public RetryOption? RetryOption { get; set; }
        
        public bool UseHealthCheck { get; set; }
        public HealthCheckOption? HealthCheckOption { get; set; }

        public override void Validate()
        {
            if (OptionKey.IsEmpty())
                AddValidation(MessageHelper.NullOrEmpty(MessageEventName.OptionsValidation,
                    nameof(RedisDbOption), nameof(OptionKey)));
            if (PoolSize <= 0)
                AddValidation(MessageHelper.MustBeGreaterThan(MessageEventName.OptionsValidation,
                    nameof(RedisDbOption), nameof(PoolSize), PoolSize, 0));

            if (Database < 0)
                AddValidation(MessageHelper.MustBeGreaterThan(MessageEventName.OptionsValidation,
                    nameof(RedisDbOption), nameof(Database), Database, 0));

            if (ConnectTimeout <= 0)
                AddValidation(MessageHelper.MustBeGreaterThan(MessageEventName.OptionsValidation,
                    nameof(RedisDbOption), nameof(ConnectTimeout), ConnectTimeout, 0));

            if (SyncTimeout <= 0)
                AddValidation(MessageHelper.MustBeGreaterThan(MessageEventName.OptionsValidation,
                    nameof(RedisDbOption), nameof(SyncTimeout), SyncTimeout, 0));

            if (Ssl)
            {
                if (Password.IsEmpty())
                    AddValidation(MessageHelper.NullOrEmpty(MessageEventName.OptionsValidation,
                        nameof(RedisDbOption), nameof(Password)));
            }

            if (UseRetry)
            {
                RetryOption ??= new RetryOption();
                RetryOption.Validate();
            }

            if (UseHealthCheck)
            {
                HealthCheckOption ??= new HealthCheckOption();
                HealthCheckOption.Validate();
            }

            if (ConnectionString.IsNotEmpty()) return;
            if (Hosts.Length == 0)
                AddValidation(MessageHelper.NullOrEmpty(MessageEventName.OptionsValidation,
                    nameof(RedisDbOption), nameof(Hosts)));
        }
    }
}