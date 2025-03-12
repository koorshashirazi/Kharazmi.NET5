using Kharazmi.BuilderExtensions;
using Kharazmi.Builders;
using Kharazmi.Constants;
using Kharazmi.Exceptions;
using Kharazmi.Extensions;
using Kharazmi.HealthChecks;
using Kharazmi.Options.Hangfire;
using Kharazmi.Options.HealthCheck;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Kharazmi.Hangfire.HealthChecks
{
    /// <summary>_</summary>
    public static class HangfireHealthCheckExtensions
    {
        /// <summary>_</summary>
        /// <param name="configurePluginBuilder"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        internal static void AddHangfire(this HealthCheckConfigurePluginBuilder configurePluginBuilder,
            HangfireOption option)
        {
            if (option.IsNull() || option.UseHealthCheck == false) return;

            switch (option.JobStorageType)
            {
                case JobStorageTypes.MongoDb:
                    var mongoStorageOption = option.HangfireMongoStorageOption;
                    if (mongoStorageOption.IsNull())
                        throw new NotFoundOptionException(nameof(HangfireOption), nameof(HangfireMongoStorageOption));

                    if (mongoStorageOption.UseHealthCheck == false) break;
                    
                    var registry1 = HealthServiceTypeRegistry.Instance;
                    registry1.RegisterForNestedOption<HangfireMongoStorageOption, IHangfireMongoHealthCheck>();

                    configurePluginBuilder.Services.AddSingleton<IHangfireMongoHealthCheck>(sp =>
                        new HangfireMongoHealthCheck(
                            sp.GetSettings().Get<HangfireOption>().HangfireMongoStorageOption,
                            sp.GetService<ILogger<HangfireMongoHealthCheck>>()));

                    var mongoHealthOption = mongoStorageOption.HealthCheckOption ?? HealthCheckOption.Empty;

                    configurePluginBuilder.Services.RegisterHealthCheck(new HealthCheckRegistration(
                        mongoHealthOption.Name,
                        sp => sp.GetRequiredService<IHangfireMongoHealthCheck>(),
                        HealthStatus.Unhealthy, mongoHealthOption.Tags, mongoHealthOption.CheckTimeOut));
                    break;
                case JobStorageTypes.Redis:
                    var redisStorageOption = option.HangfireRedisStorageOption;
                    if (redisStorageOption.IsNull())
                        throw new NotFoundOptionException(nameof(HangfireOption), nameof(HangfireRedisStorageOption));

                    if (redisStorageOption.UseHealthCheck == false) break;

                    var registry2 = HealthServiceTypeRegistry.Instance;
                    registry2.RegisterForNestedOption<HangfireRedisStorageOption, IHangfireRedisHealthCheck>();

                    configurePluginBuilder.Services.AddSingleton<IHangfireRedisHealthCheck>(sp =>
                        new HangfireRedisHealthCheck(
                            sp.GetSettings().Get<HangfireOption>().HangfireRedisStorageOption,
                            sp.GetService<ILogger<HangfireRedisHealthCheck>>()));

                    var redisHealthOption = redisStorageOption.HealthCheckOption ?? HealthCheckOption.Empty;
                    
                    configurePluginBuilder.Add(new HealthCheckRegistration(redisHealthOption.Name,
                        sp => sp.GetRequiredService<IHangfireRedisHealthCheck>(),
                        HealthStatus.Unhealthy, redisHealthOption.Tags, redisHealthOption.CheckTimeOut));
                    break;
            }

            var healthOption = option.HealthCheckOption ?? HealthCheckOption.Empty;
            var registry = HealthServiceTypeRegistry.Instance;
            registry.RegisterForOption<HangfireOption, IHangfireHealthCheck>();

            configurePluginBuilder.Services.AddSingleton<IHangfireHealthCheck>(sp => new HangfireHealthCheck(
                sp.GetSettings().Get<HangfireOption>(),
                sp.GetService<ILogger<HangfireHealthCheck>>()));


            configurePluginBuilder.Add(new HealthCheckRegistration(healthOption.Name,
                sp => sp.GetRequiredService<IHangfireHealthCheck>(),
                HealthStatus.Unhealthy, healthOption.Tags, healthOption.CheckTimeOut));
        }
    }
}