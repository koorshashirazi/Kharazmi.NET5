using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using Kharazmi.Constants;
using Kharazmi.Exceptions;
using Kharazmi.Extensions;
using Kharazmi.HealthChecks;
using Kharazmi.Models;
using Kharazmi.Options.Domain;
using Kharazmi.Options.HealthCheck;

namespace Kharazmi.Options.RabbitMq
{
    /// <summary> </summary>
    public class RabbitMqOption : ConfigurePluginOption, IHaveHealthCheckOption
    {
        private readonly HashSet<string> _hostNames;
        private TimeSpan _requestTimeout;
        private TimeSpan _publishConfirmTimeout;
        private TimeSpan _gracefulShutdown;
        private TimeSpan _recoveryInterval;
        private bool _useBusHandler;
        private bool _useAutoModelBuilder;

        public RabbitMqOption()
        {
            _hostNames = new HashSet<string>
            {
                "localhost"
            };
            Port = 5672;
            VirtualHost = "/";
            Username = "guest";
            Password = "guest";
            ExchangeName = "default";
            _requestTimeout = TimeSpan.FromSeconds(10.0);
            _publishConfirmTimeout = TimeSpan.FromSeconds(1.0);
            _recoveryInterval = TimeSpan.FromSeconds(10.0);
            _gracefulShutdown = TimeSpan.FromSeconds(10.0);
            PersistentDeliveryMode = true;
            AutoCloseConnection = true;
            AutomaticRecovery = true;
            TopologyRecovery = true;
            RouteWithGlobalId = true;
            Exchange = new GeneralExchangeConfiguration();
            Queue = new GeneralQueueConfiguration();
            PublishAsynchronous = false;
            SubscribeAsynchronous = false;
            ThrowExceptionOnSubscribeFailed = true;
            MessageNamingConventions = new List<MessageNamingConventions>();
            ModelBuilderStrategy = RabbitMqModelBuilderStrategies.Ignore;
            ApiBaseUrl = "http://localhost:15672";
            DefaultPlugins = new HashSet<StringBoolean>();
        }


        public bool UseBusHandler
        {
            get => Enable && _useBusHandler;
            set => _useBusHandler = value;
        }

        public bool UseAutoModelBuilder
        {
            get => Enable && _useAutoModelBuilder;
            set => _useAutoModelBuilder = value;
        }

        public GeneralExchangeConfiguration Exchange { get; set; }

        public GeneralQueueConfiguration Queue { get; set; }

        public bool ThrowOnModelBuild { get; set; }

        public string ModelBuilderStrategy { get; set; }
        public IReadOnlyCollection<string> ModelBuilderStrategies => RabbitMqModelBuilderStrategies.GetStrategies();
        public string ApiBaseUrl { get; set; }
        public string ExchangeName { get; set; }
        public bool PublishAsynchronous { get; set; }
        public bool SubscribeAsynchronous { get; set; }
        public bool ThrowExceptionOnSubscribeFailed { get; set; }

        public bool UseRetryOption { get; set; }
        public RetryOption? RetryOption { get; set; }

        /// <summary> </summary>
        public bool WithRequeuing { get; set; }

        public TimeSpan RequestTimeout
        {
            get => _requestTimeout;
            set
            {
                if (value <= TimeSpan.Zero && value != Timeout.InfiniteTimeSpan ||
                    value.TotalMilliseconds > int.MaxValue)
                    throw new OptionValidationException(MessageHelper.MaxAllowed(MessageEventName.OptionsValidation,
                        nameof(RabbitMqOption), nameof(RequestTimeout), value));

                _requestTimeout = value;
            }
        }

        public TimeSpan PublishConfirmTimeout
        {
            get => _publishConfirmTimeout;
            set
            {
                if (value <= TimeSpan.Zero && value != Timeout.InfiniteTimeSpan ||
                    value.TotalMilliseconds > int.MaxValue)
                    throw new OptionValidationException(MessageHelper.MaxAllowed(MessageEventName.OptionsValidation,
                        nameof(RabbitMqOption), nameof(PublishConfirmTimeout), value));

                _publishConfirmTimeout = value;
            }
        }

        public TimeSpan GracefulShutdown
        {
            get => _gracefulShutdown;
            set
            {
                if (value <= TimeSpan.Zero && value != Timeout.InfiniteTimeSpan ||
                    value.TotalMilliseconds > int.MaxValue)
                    throw new OptionValidationException(MessageHelper.MaxAllowed(MessageEventName.OptionsValidation,
                        nameof(RabbitMqOption), nameof(GracefulShutdown), value));

                _gracefulShutdown = value;
            }
        }

        public TimeSpan RecoveryInterval
        {
            get => _recoveryInterval;
            set
            {
                if (value <= TimeSpan.Zero && value != Timeout.InfiniteTimeSpan ||
                    value.TotalMilliseconds > int.MaxValue)
                    throw new OptionValidationException(MessageHelper.MaxAllowed(MessageEventName.OptionsValidation,
                        nameof(RabbitMqOption), nameof(RecoveryInterval), value));

                _recoveryInterval = value;
            }
        }

        public bool RouteWithGlobalId { get; set; }

        public bool AutomaticRecovery { get; set; }

        public bool TopologyRecovery { get; set; }

        public bool PersistentDeliveryMode { get; set; }

        public bool AutoCloseConnection { get; set; }

        public bool UseSsl { get; set; }

        public SslOption? Ssl { get; set; }

        [StringLength(20)] public string VirtualHost { get; set; }

        [StringLength(20)] public string Username { get; set; }

        [StringLength(20)] public string Password { get; set; }

        public int Port { get; set; }

        public string[] HostNames
        {
            get => _hostNames.ToArray();
            set
            {
                if (value.Length <= 0) return;
                foreach (var val in value)
                    _hostNames.Add(val);
            }
        }

        /// <summary> </summary>
        public List<MessageNamingConventions> MessageNamingConventions { get; set; }

        public bool UseHealthCheck { get; set; }
        public HealthCheckOption? HealthCheckOption { get; set; }

        public HashSet<StringBoolean> DefaultPlugins { get; set; }

        public override void Validate()
        {
            if (VirtualHost.IsEmpty())
                AddValidation(MessageHelper.NullOrEmpty(MessageEventName.OptionsValidation,
                    nameof(RabbitMqOption), nameof(VirtualHost)));

            if (Username.IsEmpty())
                AddValidation(MessageHelper.NullOrEmpty(MessageEventName.OptionsValidation,
                    nameof(RabbitMqOption), nameof(Username)));

            if (Password.IsEmpty())
                AddValidation(MessageHelper.NullOrEmpty(MessageEventName.OptionsValidation,
                    nameof(RabbitMqOption), nameof(Password)));


            if (_recoveryInterval <= TimeSpan.Zero && _recoveryInterval != Timeout.InfiniteTimeSpan ||
                _recoveryInterval.TotalMilliseconds > int.MaxValue)
                AddValidation(MessageHelper.MaxAllowed(MessageEventName.OptionsValidation,
                    nameof(RabbitMqOption), nameof(RecoveryInterval), _recoveryInterval));

            if (_gracefulShutdown <= TimeSpan.Zero && _gracefulShutdown != Timeout.InfiniteTimeSpan ||
                _gracefulShutdown.TotalMilliseconds > int.MaxValue)
                AddValidation(MessageHelper.MaxAllowed(MessageEventName.OptionsValidation,
                    nameof(RabbitMqOption), nameof(GracefulShutdown), _gracefulShutdown));

            if (_publishConfirmTimeout <= TimeSpan.Zero && _publishConfirmTimeout != Timeout.InfiniteTimeSpan ||
                _publishConfirmTimeout.TotalMilliseconds > int.MaxValue)
                AddValidation(MessageHelper.MaxAllowed(MessageEventName.OptionsValidation,
                    nameof(RabbitMqOption), nameof(PublishConfirmTimeout), _publishConfirmTimeout));

            if (_requestTimeout <= TimeSpan.Zero && _requestTimeout != Timeout.InfiniteTimeSpan ||
                _requestTimeout.TotalMilliseconds > int.MaxValue)
                AddValidation(MessageHelper.MaxAllowed(MessageEventName.OptionsValidation,
                    nameof(RabbitMqOption), nameof(RequestTimeout), _requestTimeout));

            MessageNamingConventions.ForEach(x => x.Validate());

            if (DefaultPlugins.Any() == false)
            {
                DefaultPlugins.Add(new StringBoolean(RabbitMqDefaultPluginName.UseAttributeRouting, false));
                DefaultPlugins.Add(new StringBoolean(RabbitMqDefaultPluginName.UseRetryLater, false));
                DefaultPlugins.Add(new StringBoolean(RabbitMqDefaultPluginName.UseRetryLater, false));
                DefaultPlugins.Add(new StringBoolean(RabbitMqDefaultPluginName.UseRetryStrategy, false));
                DefaultPlugins.Add(new StringBoolean(RabbitMqDefaultPluginName.UseAvoidDuplicateMessage, false));
                DefaultPlugins.Add(new StringBoolean(RabbitMqDefaultPluginName.UseMessageContext, false));
                DefaultPlugins.Add(new StringBoolean(RabbitMqDefaultPluginName.UseContextForwarding, false));
            }

            if (UseSsl)
            {
                Ssl ??= new SslOption();
                Ssl.Validate();
            }

            if (UseRetryOption)
            {
                RetryOption ??= new RetryOption();
                RetryOption.Validate();
            }

            if (!UseHealthCheck) return;
            HealthCheckOption ??= new HealthCheckOption();
            HealthCheckOption.Validate();
        }
    }
}