using Kharazmi.Builders;
using Kharazmi.Configuration;
using Kharazmi.ConfigurePlugins;
using Kharazmi.Hangfire.HealthChecks;
using Kharazmi.Options.Hangfire;

namespace Kharazmi.Hangfire.ConfigurePlugins
{
    internal class HangfireHealthCheckConfigurePlugin : IHealthCheckConfigurePlugin
    {
        public string HealthCheckName => nameof(HangfireHealthCheckConfigurePlugin);

        public void Initialize(HealthCheckConfigurePluginBuilder configurePluginBuilder, ISettingProvider settings)
        {
            var options = settings.Get<HangfireOption>();

            if (options.Enable == false) return;
            configurePluginBuilder.AddHangfire(options);
        }
    }
}