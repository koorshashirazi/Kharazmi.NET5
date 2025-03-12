using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Kharazmi.Configuration;
using Kharazmi.Constants;
using Kharazmi.Dependency;
using Kharazmi.Extensions;
using Kharazmi.Guard;
using Kharazmi.Options;
using Kharazmi.Options.Redis;
using Kharazmi.Redis.Extensions;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Core.Implementations;
using StackExchange.Redis.Extensions.Core.ServerIteration;

namespace Kharazmi.Redis
{
    /// <summary>_</summary>
    public interface IRedisPool : IShouldBeSingleton, IMustBeInstance
    {
        /// <summary>_</summary>
        IRedisDatabase RedisDatabase(RedisDbOption option);

        /// <summary>_</summary>
        IConnectionMultiplexer Connection(RedisDbOption option);

        IServer[] GerServers(RedisDbOption option);
    }

    internal class RedisPool : IRedisPool
    {
        private readonly ILogger<RedisPool>? _logger;
        private readonly ConcurrentDictionary<string, IRedisCacheConnectionPoolManager> _connectionPools = new();
        private readonly ConcurrentDictionary<string, IRedisDatabase> _dbPool = new();
        private readonly ISerializer _serializer;

        public RedisPool(
            ISettingProvider settings,
            [MaybeNull] ILogger<RedisPool>? logger,
            ISerializer serializer)
        {
            _logger = logger;
            _serializer = serializer;
            settings.UpdatedOptionHandler += OnUpdatedOptionHandler;
        }

        public IRedisDatabase RedisDatabase(RedisDbOption option)
        {
            return _dbPool.GetOrAdd(option.OptionKey, _ =>
            {
                var config = MapRedisConfiguration(option);
                var connection = ConnectionFor(option);

                return new RedisDatabase(connection, _serializer, config.ServerEnumerationStrategy,
                    config.Database, config.MaxValueLength, config.KeyPrefix);
            });
        }

        public IConnectionMultiplexer Connection(RedisDbOption option)
        {
            var connection = ConnectionFor(option);
            return connection.GetConnection();
        }

        public IServer[] GerServers(RedisDbOption option)
        {
            var multiplexer = Connection(option);
            var serverStrategy = option.ServerEnumerationStrategy.MapServerEnumerationStrategy();
            return ServerIteratorFactory.GetServers(multiplexer, serverStrategy).ToArray();
        }

        private static RedisConfiguration MapRedisConfiguration(RedisDbOption option)
        {
            option.ServerEnumerationStrategy.MapServerEnumerationStrategy();
            option.DefaultPatternMode.MapPatternMode();
            option.DefaultCommandFlags.MapCommandFlag();
            option.SslProtocols.MapSslProtocols();
            var config = option.MapTo<RedisConfiguration>().NotNull(nameof(RedisConfiguration));
            return config;
        }

        private IRedisCacheConnectionPoolManager ConnectionFor(RedisDbOption option)
        {
            return _connectionPools.GetOrAdd(option.OptionKey, _ =>
            {
                var config = MapRedisConfiguration(option);
                return new RedisConnectionManager(config, _logger);
            });
        }

        private void OnUpdatedOptionHandler(ISettingProvider sender, IOptions options, Type optionType)
        {
            if (!(options is RedisDbOptions redisDbOptions)) return;

            foreach (var option in redisDbOptions.ChildOptions)
            {
                if (!option.IsDirty) continue;
                _logger?.LogTrace(MessageTemplate.OptionChanged, MessageEventName.OptionChanged,
                    nameof(RedisPool), option.GetType().Name);
                _connectionPools.TryRemove(option.OptionKey, out _);
            }
        }
    }
}