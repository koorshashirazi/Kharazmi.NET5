using Kharazmi.BuilderExtensions;
using Kharazmi.Builders;
using Kharazmi.HealthChecks;
using Kharazmi.Options.HealthCheck;
using Kharazmi.Options.RabbitMq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Kharazmi.RabbitMq.HealthChecks
{
    /// <summary>_</summary>
    public static class RabbitMqHealthCheckExtensions
    {
        /// <summary>_</summary>
        /// <param name="configurePluginBuilder"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        public static void AddRabbitMq(this HealthCheckConfigurePluginBuilder configurePluginBuilder, RabbitMqOption option)
        {
            if (!option.UseHealthCheck) return;

            var registry = HealthServiceTypeRegistry.Instance;
            registry.RegisterForOption<RabbitMqOption, IRabbitMqHealthCheck>();

            configurePluginBuilder.Services.AddSingleton<IRabbitMqHealthCheck>(sp =>
                new RabbitMqHealthCheck(sp.GetSettings(), sp.GetService<ILogger<RabbitMqHealthCheck>>()));

            var healthOption = option.HealthCheckOption ?? HealthCheckOption.Empty;

            configurePluginBuilder.Add(new HealthCheckRegistration(healthOption.Name,
                sp => sp.GetRequiredService<IRabbitMqHealthCheck>(),
                HealthStatus.Unhealthy, healthOption.Tags, healthOption.CheckTimeOut));
        }
    }
}