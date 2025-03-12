using Kharazmi.BuilderExtensions;
using Kharazmi.Builders;
using Kharazmi.HealthChecks;
using Kharazmi.Options.HealthCheck;
using Kharazmi.Options.Redis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Kharazmi.Redis.HealthChecks
{
    /// <summary></summary>
    public static class RedisHealthCheckExtensions
    {
        /// <summary></summary>
        /// <param name="configurePluginBuilder"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        public static void AddRedis(this HealthCheckConfigurePluginBuilder configurePluginBuilder, RedisDbOption option)
        {
            if (option.UseHealthCheck == false) return;

            var registry = HealthServiceTypeRegistry.Instance;
            registry.RegisterForChildOption<RedisDbOption, IRedisHealthCheck>();

            configurePluginBuilder.Services.AddSingleton<IRedisHealthCheck>(sp =>
                new RedisHealthCheck(sp.GetSettings(), sp.GetService<ILogger<RedisHealthCheck>>()));

            var healthOption = option.HealthCheckOption ?? HealthCheckOption.Empty;

            configurePluginBuilder.Add(new HealthCheckRegistration(healthOption.Name,
                sp => sp.GetRequiredService<IRedisHealthCheck>(),
                HealthStatus.Healthy, healthOption.Tags, healthOption.CheckTimeOut));
        }
    }
}