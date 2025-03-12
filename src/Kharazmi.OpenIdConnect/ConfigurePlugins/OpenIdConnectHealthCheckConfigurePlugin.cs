using Kharazmi.Builders;
using Kharazmi.Configuration;
using Kharazmi.ConfigurePlugins;
using Kharazmi.OpenIdConnect.HealthChecks;
using Kharazmi.Options.Cookie;

namespace Kharazmi.OpenIdConnect.ConfigurePlugins
{
    internal class OpenIdConnectHealthCheckConfigurePlugin : IHealthCheckConfigurePlugin
    {
        public string HealthCheckName => nameof(OpenIdConnectHealthCheckConfigurePlugin);

        public void Initialize(HealthCheckConfigurePluginBuilder configurePluginBuilder, ISettingProvider settings)
        {
            var option = settings.Get<ExtendedOpenIdOption>();
            if (option.Enable == false) return;

            configurePluginBuilder.AddOpenIdConnectServer(option);
        }
    }
}