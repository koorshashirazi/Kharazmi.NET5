using Kharazmi.Builders;
using Kharazmi.Configuration;
using Kharazmi.ConfigurePlugins;
using Kharazmi.Options.Redis;
using Kharazmi.Redis.HealthChecks;

namespace Kharazmi.Redis.ConfigurePlugins
{
    internal class RedisHealthCheckConfigurePlugin : IHealthCheckConfigurePlugin
    {
        public string HealthCheckName => nameof(RedisHealthCheckConfigurePlugin);

        public void Initialize(HealthCheckConfigurePluginBuilder configurePluginBuilder, ISettingProvider settings)
        {
            var options = settings.Get<RedisDbOptions>();
            if (options.Enable == false) return;

            foreach (var option in options.ChildOptions)
                configurePluginBuilder.AddRedis(option);
        }
    }
}