using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Kharazmi.Constants;
using Kharazmi.Extensions;

namespace Kharazmi.Options.Redis
{
    /// <summary>_</summary>
    public class ServerEnumerationStrategy
    {
        private readonly HashSet<string> _modeOptions;
        private readonly HashSet<string> _targetRoleOptions;
        private readonly HashSet<string> _unreachableServerActionOptions;

        public ServerEnumerationStrategy()
        {
            Mode = ModeOption.All;
            TargetRole = TargetRoleOption.Any;
            UnreachableServerAction = UnreachableServerActionOption.Throw;

            _modeOptions = new HashSet<string>
            {
                ModeOption.All,
                ModeOption.Single,
            };
            _targetRoleOptions = new HashSet<string>
            {
                TargetRoleOption.Any,
                TargetRoleOption.PreferSlave,
            };
            _unreachableServerActionOptions = new HashSet<string>
            {
                UnreachableServerActionOption.Throw,
                UnreachableServerActionOption.IgnoreIfOtherAvailable,
            };
        }

        /// <summary>_</summary>
        public string Mode { get; set; }

        public IReadOnlyCollection<string> Modes => _modeOptions;

        /// <summary>_</summary>
        public string TargetRole { get; set; }

        public IReadOnlyCollection<string> TargetRoles => _targetRoleOptions;


        /// <summary>_</summary>
        /// <summary>_</summary>
        public string UnreachableServerAction { get; set; }

        public IReadOnlyCollection<string> UnreachableServerActions => _unreachableServerActionOptions;
    }

    /// <summary>_ </summary>
    public class RedisHost
    {
        public RedisHost()
        {
            Host = "localhost";
            Port = 6379;
        }

        /// <summary>_</summary>
        [StringLength(100)]
        public string Host { get; set; }


        /// <summary>_</summary>
        public int Port { get; set; }
    }


    public class RedisDbOptions : ConfigurePluginOptionWithChild<RedisDbOption>
    {
        private bool _useChannelPublisher;

        public RedisDbOptions()
        {
        }

        public bool UseChannelPublisher
        {
            get => Enable && _useChannelPublisher;
            set => _useChannelPublisher = value;
        }

        public string DefaultOption { get; set; }

        public override void Validate()
        {
            foreach (var option in ChildOptions)
            {
                option.Validate();
                AddValidations(option.ValidationResults());
            }

            if (DefaultOption.IsEmpty())
                DefaultOption = ChildOptions.First().OptionKey;
        }
    }
}