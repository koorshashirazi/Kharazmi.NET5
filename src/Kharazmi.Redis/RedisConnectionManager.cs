using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Core.Models;

namespace Kharazmi.Redis
{
    internal class RedisConnectionManager : IRedisCacheConnectionPoolManager
    {
        private readonly ConcurrentBag<Lazy<StateAwareConnection>> _connections;
        private readonly ILogger? _logger;
        private readonly RedisConfiguration _config;


        public RedisConnectionManager(RedisConfiguration config, [MaybeNull] ILogger? logger = null)
        {
            _config = config;
            _connections = new ConcurrentBag<Lazy<StateAwareConnection>>();
            _logger = logger ?? NullLogger<RedisConnectionManager>.Instance;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            var activeConnections = _connections.Where(lazy => lazy.IsValueCreated).ToList();

            foreach (var connection in activeConnections)
                ((ConnectionMultiplexer) connection.Value).Dispose();

            while (_connections.IsEmpty == false)
                _connections.TryTake(out _);
        }

        /// <inheritdoc/>
        public IConnectionMultiplexer GetConnection()
        {
            EmitConnections();

            var loadedLazies = _connections.Where(lazy => lazy.IsValueCreated);

            if (loadedLazies.Count() == _connections.Count)
                return (ConnectionMultiplexer) _connections.OrderBy(x => x.Value.TotalOutstanding()).First().Value;

            return (ConnectionMultiplexer) _connections.First(lazy => !lazy.IsValueCreated).Value;
        }

        /// <inheritdoc/>
        public ConnectionPoolInformation GetConnectionInformations()
        {
            var activeConnections = 0;
            var invalidConnections = 0;
            var readyNotUsedYet = 0;

            foreach (var lazy in _connections)
            {
                if (!lazy.IsValueCreated)
                {
                    readyNotUsedYet++;
                    continue;
                }

                if (!lazy.Value.IsConnected())
                {
                    invalidConnections++;
                    continue;
                }

                activeConnections++;
            }

            return new ConnectionPoolInformation()
            {
                RequiredPoolSize = _config.PoolSize,
                ActiveConnections = activeConnections,
                InvalidConnections = invalidConnections,
                ReadyNotUsedYet = readyNotUsedYet
            };
        }

        private void EmitConnection()
        {
            _connections.Add(new Lazy<StateAwareConnection>(() =>
            {
                _logger?.LogTrace("Creating new Redis connection");

                var multiplexer = ConnectionMultiplexer.Connect(_config.ConfigurationOptions);

                if (_config.ProfilingSessionProvider != null)
                    multiplexer.RegisterProfiler(_config.ProfilingSessionProvider);

                return new StateAwareConnection(multiplexer, _logger);
            }));
        }

        private void EmitConnections()
        {
            if (_connections.Count >= _config.PoolSize)
                return;

            for (var i = 0; i < _config.PoolSize; i++)
            {
                _logger?.LogTrace("Creating the redis connection pool with {PoolSize} _connections",
                    _config.PoolSize);
                EmitConnection();
            }
        }

        internal sealed class StateAwareConnection
        {
            private readonly ConnectionMultiplexer multiplexer;
            private readonly ILogger? _logger;

            public StateAwareConnection(ConnectionMultiplexer multiplexer, [MaybeNull] ILogger? logger)
            {
                this.multiplexer = multiplexer ?? throw new ArgumentNullException(nameof(multiplexer));

                this.multiplexer.ConnectionFailed += ConnectionFailed;
                this.multiplexer.ConnectionRestored += ConnectionRestored;
                _logger = logger;
            }

            public static implicit operator ConnectionMultiplexer(StateAwareConnection c) => c.multiplexer;

            public long TotalOutstanding() => multiplexer.GetCounters().TotalOutstanding;

            public bool IsConnected() => multiplexer.IsConnecting == false;

            private void ConnectionFailed(object sender, ConnectionFailedEventArgs e)
            {
                _logger?.LogError(e.Exception, "Redis connection error {FailureType}", e.FailureType);
            }

            private void ConnectionRestored(object sender, ConnectionFailedEventArgs e)
            {
                _logger?.LogError("Redis connection error restored");
            }
        }
    }
}