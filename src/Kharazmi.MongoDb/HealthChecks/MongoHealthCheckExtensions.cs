using Kharazmi.BuilderExtensions;
using Kharazmi.Builders;
using Kharazmi.HealthChecks;
using Kharazmi.Options.HealthCheck;
using Kharazmi.Options.Mongo;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Kharazmi.Localization.HealthChecks
{
    public static class MongoHealthCheckExtensions
    {
        public static void AddMongo(this HealthCheckConfigurePluginBuilder configurePluginBuilder, MongoOption option)
        {
            if (option.UseHealthCheck == false) return;

            var registry = HealthServiceTypeRegistry.Instance;
            registry.RegisterForChildOption<MongoOption, IMongoHealthCheck>();

            configurePluginBuilder.Services.AddSingleton<IMongoHealthCheck>(sp =>
                new MongoHealthCheck(sp.GetSettings(), sp.GetService<ILogger<MongoHealthCheck>>()));

            var healthOption = option.HealthCheckOption ?? HealthCheckOption.Empty;

            configurePluginBuilder.Add(new HealthCheckRegistration(healthOption.Name,
                sp => sp.GetRequiredService<IMongoHealthCheck>(),
                HealthStatus.Unhealthy, healthOption.Tags, healthOption.CheckTimeOut));
        }
    }
}