using Kharazmi.BuilderExtensions;
using Kharazmi.Builders;
using Kharazmi.HealthChecks;
using Kharazmi.Options.Cookie;
using Kharazmi.Options.HealthCheck;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Kharazmi.OpenIdConnect.HealthChecks
{
    /// <summary> </summary>
    public static class OpenIdConnectServerHealthCheckExtensions
    {
        /// <summary>_</summary>
        /// <param name="configurePluginBuilder"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        public static void AddOpenIdConnectServer(this HealthCheckConfigurePluginBuilder configurePluginBuilder,
            ExtendedOpenIdOption option)
        {
            if (option.UseHealthCheck == false) return;
            var healthOption = option.HealthCheckOption ?? HealthCheckOption.Empty;

            var registry = HealthServiceTypeRegistry.Instance;
            registry.RegisterForOption<ExtendedOpenIdOption, IOpenIdConnectServerHealthCheck>();

            configurePluginBuilder.Services.AddSingleton<IOpenIdConnectServerHealthCheck>(sp =>
                new OpenIdConnectServerHealthCheck(sp.GetSettings(), sp.GetHttpClientFactory(),
                    sp.GetService<ILogger<OpenIdConnectServerHealthCheck>>()));

            configurePluginBuilder.Add(new HealthCheckRegistration(healthOption.Name,
                sp => sp.GetRequiredService<IOpenIdConnectServerHealthCheck>(),
                HealthStatus.Unhealthy, healthOption.Tags, healthOption.CheckTimeOut));
        }
    }
}