using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Constants;
using Kharazmi.Options.Hangfire;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Kharazmi.Hangfire.HealthChecks
{
    internal class HangfireHealthCheck : IHangfireHealthCheck
    {
        private static int _attempt;
        private readonly HangfireOption? _options;
        private readonly ILogger<HangfireHealthCheck>? _logger;

        public HangfireHealthCheck(
            [AllowNull] HangfireOption? options,
            [AllowNull] ILogger<HangfireHealthCheck>? logger)
        {
            _options = options;
            _logger = logger;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken token = default)
            => CheckHealthAsync(_options, context, token);

        private Task<HealthCheckResult> CheckHealthAsync(HangfireOption? options, HealthCheckContext? context,
            CancellationToken token = default)
        {
            try
            {
#warning fix messages

                if (token.IsCancellationRequested)
                {
                    _logger?.LogWarning("The hangfire server Cancellation Requested");
                    return Task.FromResult(HealthCheckResult.Degraded("Cancellation Requested"));
                }

                if (options is null || !options.IsValid())
                {
                    _logger?.LogError(MessageTemplate.CanNotUseHangfire, MessageEventName.Hangfire,
                        nameof(HangfireHealthCheck));
                    return Task.FromResult(
                        HealthCheckResult.Unhealthy(MessageHelper.CanNotUseHangfire(nameof(HangfireHealthCheck))));
                }

                _attempt++;

                if (options.HealthCheckOption?.Attempts > 0 && _attempt > options.HealthCheckOption?.Attempts)
                {
                    _logger?.LogWarning("The hangfire server is active. Attempted to send ping {Attempt} times",
                        _attempt);
                    return Task.FromResult(
                        HealthCheckResult.Degraded(
                            $"The hangfire server is active. Attempted to send ping {_attempt} times"));
                }

                var errorList = new List<string>();
                var hangfireMonitoringApi = global::Hangfire.JobStorage.Current.GetMonitoringApi();

                if (options.MaximumJobsFailed.HasValue && options.MaximumJobsFailed != -1)
                {
                    var failedJobsCount = hangfireMonitoringApi.FailedCount();
                    if (failedJobsCount >= options.MaximumJobsFailed)
                    {
                        errorList.Add(
                            $"Hangfire has #{failedJobsCount} failed jobs and the maximum available is {options.MaximumJobsFailed}.");
                    }
                }

                if (options.MinimumAvailableServers.HasValue && options.MinimumAvailableServers != -1)
                {
                    var serversCount = hangfireMonitoringApi.Servers().Count;
                    if (serversCount < options.MinimumAvailableServers)
                    {
                        errorList.Add(
                            $"{serversCount} server registered. Expected minimum {options.MinimumAvailableServers}.");
                    }
                }

                if (errorList.Any())
                {
                    _logger?.LogError("{ErrorList}", string.Join(" + ", errorList));
                    return Task.FromResult(HealthCheckResult.Unhealthy(string.Join(" + ", errorList)));
                }

                _logger?.LogTrace("The Hangfire server is health. Attempted {Attempt} times", _attempt);
                return Task.FromResult(
                    HealthCheckResult.Healthy($"The Hangfire server is health. Attempted {_attempt} times"));
            }
            catch (Exception ex)
            {
                _logger?.LogError("{Message}", ex.Message);
                return Task.FromResult(HealthCheckResult.Unhealthy(ex.Message));
            }
        }
    }
}