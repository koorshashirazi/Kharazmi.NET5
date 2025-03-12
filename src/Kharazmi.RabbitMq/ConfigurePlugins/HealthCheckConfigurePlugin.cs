using Kharazmi.Builders;
using Kharazmi.Configuration;
using Kharazmi.ConfigurePlugins;
using Kharazmi.Options.RabbitMq;
using Kharazmi.RabbitMq.HealthChecks;

namespace Kharazmi.RabbitMq.ConfigurePlugins
{
    internal class HealthCheckConfigurePlugin : IHealthCheckConfigurePlugin
    {
        public string HealthCheckName => nameof(HealthCheckConfigurePlugin);

        public void Initialize(HealthCheckConfigurePluginBuilder configurePluginBuilder, ISettingProvider settings)
        {
            var options = settings.Get<RabbitMqOption>();
            if (options.Enable == false) return;
            configurePluginBuilder.AddRabbitMq(options);
        }
    }
}