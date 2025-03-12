using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Constants;
using Kharazmi.Extensions;
using Kharazmi.Options.Hangfire;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Kharazmi.Hangfire.HealthChecks
{
    /// <summary>_</summary>
    internal class HangfireRedisHealthCheck : IHangfireRedisHealthCheck
    {
        private static int _attempt;
        private readonly HangfireRedisStorageOption? _options;
        private readonly ILogger<HangfireRedisHealthCheck>? _logger;

        private static readonly ConcurrentDictionary<string, ConnectionMultiplexer> Connections = new();

        /// <summary>_</summary>
        public HangfireRedisHealthCheck(HangfireRedisStorageOption? options, ILogger<HangfireRedisHealthCheck>? logger)
        {
            _options = options;
            _logger = logger;
        }

        /// <summary>_</summary>
        /// <param name="context"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken token = new())
            => CheckHealthAsync(_options, context, token);

        private async Task<HealthCheckResult> CheckHealthAsync(HangfireRedisStorageOption? options,
            HealthCheckContext? context,
            CancellationToken token = default)
        {
#warning Fix Messages
            if (token.IsCancellationRequested)
            {
                _logger?.LogWarning("The Hangfire Redis server is Cancelled");
                return HealthCheckResult.Degraded();
            }

            try
            {
                if (options is null || !options.IsValid())
                {
                    _logger?.LogError(MessageTemplate.CanNotUseHangfireStorage, MessageEventName.Hangfire,
                        nameof(HangfireRedisHealthCheck), nameof(JobStorageTypes.Redis));
                    return HealthCheckResult.Unhealthy(
                        MessageHelper.CanNotUseHangfireStorage(nameof(HangfireRedisHealthCheck), JobStorageTypes.Redis));
                }

                _attempt++;

                if (options.HealthCheckOption.Attempts > 0 &&
                    _attempt >= options.HealthCheckOption?.Attempts)
                {
                    _logger?.LogTrace("The Hangfire Redis server is active. Attempted to send ping {Attempt} times",
                        _attempt);
                    return HealthCheckResult.Healthy(
                        $"The Hangfire Redis server is active. Attempted to send ping {_attempt} times");
                }


                var configStr = options.ConnectionString;
                if (configStr.IsEmpty())
                {
                    _logger?.LogError("Hangfire can't accept invalid Redis Configuration");
                    return HealthCheckResult.Unhealthy("Hangfire can't accept invalid Redis Configuration");
                }

                if (!Connections.TryGetValue(configStr, out var connection))
                {
                    connection = await ConnectionMultiplexer.ConnectAsync(configStr);
                    if (!Connections.TryAdd(configStr, connection))
                    {
                        connection.Dispose();
                        connection = Connections[configStr];
                    }
                }

                await connection.GetDatabase().PingAsync();
                _logger?.LogTrace("Hangfire Redis server is active for host: {@Host}", connection.Configuration);

                _logger?.LogTrace("The Hangfire Redis is health. Attempted {Attempt} times", _attempt);
                return HealthCheckResult.Healthy(
                    $"The Hangfire Redis server is health. For host {connection.Configuration}. Attempted {_attempt} times");
            }
            catch (Exception e)
            {
                _logger.LogError("{Message}", e.Message);
                return context != null
                    ? new HealthCheckResult(context.Registration.FailureStatus, exception: e)
                    : HealthCheckResult.Unhealthy("Ensure hangfire redis server is running...");
            }
        }
    }
}