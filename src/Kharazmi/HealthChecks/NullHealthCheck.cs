using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Kharazmi.HealthChecks
{
    public class NullHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken cancellationToken = new())
            => Task.FromResult(HealthCheckResult.Degraded("Use health check is disabled, for requested service"));
    }
}