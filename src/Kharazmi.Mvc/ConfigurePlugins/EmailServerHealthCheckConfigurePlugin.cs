using Kharazmi.BuilderExtensions;
using Kharazmi.Builders;
using Kharazmi.Configuration;
using Kharazmi.ConfigurePlugins;
using Kharazmi.HealthChecks;
using Kharazmi.Mvc.HealthChecks;
using Kharazmi.Mvc.MailServer;
using Kharazmi.Options;
using Kharazmi.Options.HealthCheck;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Kharazmi.Mvc.ConfigurePlugins
{
    internal class EmailServerHealthCheckConfigurePlugin : IHealthCheckConfigurePlugin
    {
        public string HealthCheckName => nameof(EmailServerHealthCheckConfigurePlugin);

        public void Initialize(HealthCheckConfigurePluginBuilder configurePluginBuilder, ISettingProvider settings)
        {
            var option = settings.Get<EmailServerOption>();

            if (option.UseHealthCheck == false) return;
            var registry = HealthServiceTypeRegistry.Instance;

            registry.RegisterForOption<EmailServerOption, IMailServerHealthCheck>();

            configurePluginBuilder.Services.AddSingleton<IMailServerHealthCheck>(sp => new MailServerHealthCheck(
                sp.GetSettings(), sp.GetService<ILogger<MailServerHealthCheck>>()));

            var healthOption = option.HealthCheckOption ?? HealthCheckOption.Empty;

            configurePluginBuilder.Add(new HealthCheckRegistration(healthOption.Name,
                sp => sp.GetRequiredService<IMailServerHealthCheck>(),
                HealthStatus.Unhealthy, healthOption.Tags, healthOption.CheckTimeOut));
        }
    }
}