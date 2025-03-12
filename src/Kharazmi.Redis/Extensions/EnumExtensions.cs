using Kharazmi.Constants;
using Kharazmi.Options.Redis;
using StackExchange.Redis;

#pragma warning disable 1591

namespace Kharazmi.Redis.Extensions
{
    public static class EnumExtensions
    {
        public static CommandFlags MapCommandFlag(this string value)
        {
            return value switch
            {
                CommandFlag.None => CommandFlags.None,
                CommandFlag.DemandMaster => CommandFlags.DemandMaster,
                CommandFlag.DemandReplica => CommandFlags.DemandReplica,
                CommandFlag.NoRedirect => CommandFlags.NoRedirect,
                CommandFlag.PreferMaster => CommandFlags.PreferMaster,
                CommandFlag.PreferReplica => CommandFlags.PreferReplica,
                CommandFlag.FireAndForget => CommandFlags.FireAndForget,
                CommandFlag.NoScriptCache => CommandFlags.NoScriptCache,
                _ => CommandFlags.None
            };
        }

        public static RedisChannel.PatternMode MapPatternMode(this string value)
        {
            return value switch
            {
                PatternMode.Auto => RedisChannel.PatternMode.Auto,
                PatternMode.Literal => RedisChannel.PatternMode.Literal,
                PatternMode.Pattern => RedisChannel.PatternMode.Pattern,
                _ => RedisChannel.PatternMode.Auto
            };
        }

        public static StackExchange.Redis.Extensions.Core.Configuration.ServerEnumerationStrategy.ModeOptions
            MapModeOptions(this string value)
        {
            return value switch
            {
                ModeOption.All => StackExchange.Redis.Extensions.Core.Configuration.ServerEnumerationStrategy
                    .ModeOptions.All,
                ModeOption.Single => StackExchange.Redis.Extensions.Core.Configuration.ServerEnumerationStrategy
                    .ModeOptions.Single,
                _ => StackExchange.Redis.Extensions.Core.Configuration.ServerEnumerationStrategy.ModeOptions.All
            };
        }

        public static StackExchange.Redis.Extensions.Core.Configuration.ServerEnumerationStrategy.TargetRoleOptions
            MapTargetRoleOptions(this string value)
        {
            return value switch
            {
                TargetRoleOption.Any => StackExchange.Redis.Extensions.Core.Configuration.ServerEnumerationStrategy
                    .TargetRoleOptions.Any,
                TargetRoleOption.PreferSlave => StackExchange.Redis.Extensions.Core.Configuration
                    .ServerEnumerationStrategy.TargetRoleOptions.PreferSlave,
                _ => StackExchange.Redis.Extensions.Core.Configuration.ServerEnumerationStrategy.TargetRoleOptions.Any
            };
        }

        public static
            StackExchange.Redis.Extensions.Core.Configuration.ServerEnumerationStrategy.UnreachableServerActionOptions
            MapUnreachableServerActionOptions(this string value)
        {
            return value switch
            {
                UnreachableServerActionOption.Throw => StackExchange.Redis.Extensions.Core.Configuration
                    .ServerEnumerationStrategy.UnreachableServerActionOptions.Throw,
                UnreachableServerActionOption.IgnoreIfOtherAvailable => StackExchange.Redis.Extensions.Core
                    .Configuration.ServerEnumerationStrategy.UnreachableServerActionOptions.IgnoreIfOtherAvailable,
                _ => StackExchange.Redis.Extensions.Core.Configuration.ServerEnumerationStrategy
                    .UnreachableServerActionOptions.Throw
            };
        }

        public static StackExchange.Redis.Extensions.Core.Configuration.ServerEnumerationStrategy
            MapServerEnumerationStrategy(this ServerEnumerationStrategy value)
        {
            return new()
            {
                Mode = value.Mode.MapModeOptions(),
                TargetRole = value.TargetRole.MapTargetRoleOptions(),
                UnreachableServerAction = value.UnreachableServerAction.MapUnreachableServerActionOptions()
            };
        }
    }
}