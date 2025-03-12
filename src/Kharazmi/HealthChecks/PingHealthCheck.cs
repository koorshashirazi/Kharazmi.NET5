using System;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Options;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Kharazmi.HealthChecks
{
    public class PingHealthCheck : IHealthCheck
    {
        private static int _attempt;
        private readonly PingOption _option;

        public PingHealthCheck(PingOption option)
        {
            _option = option ?? throw new ArgumentNullException(nameof(option));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            var hostHealthCheckOptions = _option.HostHealthCheckOptions;

            try
            {
                if (_option.HealthCheckOption?.Attempts > 0 &&
                    _attempt >= _option.HealthCheckOption?.Attempts)
                {
                    return HealthCheckResult.Healthy(
                        $"Ping check for hosts is succeed. Attempted to send ping {_attempt} times");
                }

                if (hostHealthCheckOptions != null)
                    foreach (var option in hostHealthCheckOptions)
                    {
                        using var ping = new Ping();

                        if (option.Host is null) continue;
                        var pingReply = await ping.SendPingAsync(option.Host, option.Timeout);

                        if (pingReply.Status != IPStatus.Success)
                        {
                            return new HealthCheckResult(context.Registration.FailureStatus,
                                $"Ping check for host {option.Host} is failed with status reply:{pingReply.Status}");
                        }
                    }

                _attempt++;
                return HealthCheckResult.Healthy("Ping check for hosts is succeed");
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}