using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Configuration;
using Kharazmi.Extensions;
using Kharazmi.Options.Redis;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core.Configuration;

namespace Kharazmi.Redis.HealthChecks
{
    /// <summary>_</summary>
    internal class RedisHealthCheck : IRedisHealthCheck
    {
        private static int _attempt;
        private readonly ISettingProvider _settings;
        private readonly ILogger<RedisHealthCheck>? _logger;

        private static readonly ConcurrentDictionary<string, ConnectionMultiplexer> Connections = new();

        /// <summary>_</summary>
        public RedisHealthCheck(
            ISettingProvider settings,
            ILogger<RedisHealthCheck>? logger)
        {
            _settings = settings;
            _logger = logger;
        }

        /// <summary>_</summary>
        /// <param name="context"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext? context,
            CancellationToken token = default)
        {
            try
            {
                var options = _settings.Get<RedisDbOptions>();
                var option = options.ChildOptions
                    .FirstOrDefault(x => x.HealthCheckOption?.Name == context?.Registration?.Name);
                if (option is null)
                {
                    _logger?.LogError("Invalid Redis Configuration");
                    return HealthCheckResult.Unhealthy("Invalid Redis Configuration");
                }

                try
                {
                    option.Validate();
                    if (!option.IsValid())
                    {
#warning Use MessageTemplate
                        _logger?.LogError("Invalid Redis Configuration");
                        return HealthCheckResult.Unhealthy("Invalid Redis Configuration");
                    }

                    _attempt++;

                    if (option.HealthCheckOption?.Attempts > 0 &&
                        _attempt >= option.HealthCheckOption.Attempts)
                    {
                        _logger?.LogTrace("The Redis server is active. Attempted to send ping {Attempt} times",
                            _attempt);
                        return HealthCheckResult.Healthy(
                            $"The Redis server is active. Attempted to send ping {_attempt} times");
                    }

                    if (token.IsCancellationRequested)
                    {
                        _logger?.LogWarning("The Redis server is Cancelled");
                        return HealthCheckResult.Degraded();
                    }

                    var redisConfiguration = option.MapTo<RedisConfiguration>();
                    if (redisConfiguration is null)
                    {
                        _logger?.LogError("Invalid Redis Configuration");
                        return HealthCheckResult.Unhealthy("Invalid Redis Configuration");
                    }

                    var configOptions = redisConfiguration.ConfigurationOptions;
                    var configStr = configOptions?.ToString();
                    if (configStr.IsEmpty())
                    {
                        _logger?.LogError("Invalid Redis Configuration");
                        return HealthCheckResult.Unhealthy("Invalid Redis Configuration");
                    }

                    if (!Connections.TryGetValue(configStr, out var connection))
                    {
                        connection = await ConnectionMultiplexer.ConnectAsync(configOptions);
                        if (!Connections.TryAdd(configStr, connection))
                        {
                            connection.Dispose();
                            connection = Connections[configStr];
                        }
                    }

                    await connection.GetDatabase().PingAsync();
                    var hosts = string.Join("; ", redisConfiguration.Hosts.Select(x => $"{x.Host}:{x.Port}"));

                    _logger?.LogTrace("The Redis server is active for host: {@Host}", hosts);

                    return new HealthCheckResult(HealthStatus.Healthy, $"The Redis server is active for host: {hosts}");
                }
                catch (Exception e)
                {
                    _logger.LogError("{Message}", e.Message);
                    return context != null
                        ? new HealthCheckResult(context.Registration.FailureStatus, exception: e)
                        : HealthCheckResult.Unhealthy("Ensure redis server is running...");
                }
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy(ex.Message);
            }
        }
    }
}