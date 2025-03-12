using Kharazmi.Builders;
using Kharazmi.Configuration;
using Kharazmi.ConfigurePlugins;
using Kharazmi.Localization.HealthChecks;
using Kharazmi.Options.Mongo;

namespace Kharazmi.Localization.ConfigurePlugins
{
    internal class MongoHealthCheckConfigurePlugin : IHealthCheckConfigurePlugin
    {
        public string HealthCheckName => nameof(MongoHealthCheckConfigurePlugin);

        public void Initialize(HealthCheckConfigurePluginBuilder configurePluginBuilder, ISettingProvider settings)
        {
            var options = settings.Get<MongoOptions>();
            if (options.Enable == false) return;

            foreach (var option in options.ChildOptions)
                configurePluginBuilder.AddMongo(option);
        }
    }
}