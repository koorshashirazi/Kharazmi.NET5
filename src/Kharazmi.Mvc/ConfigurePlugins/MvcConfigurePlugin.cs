using Kharazmi.Configuration;
using Kharazmi.ConfigurePlugins;
using Kharazmi.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Kharazmi.Mvc.ConfigurePlugins
{
    internal class MvcConfigurePlugin : IConfigurePlugin
    {
        public string PluginName => nameof(MvcConfigurePlugin);

        public void Configure(ISettingProvider settings)
        {
            ExceptionOption option = settings.Get<ExceptionOption>();

            settings.UpdateOption(option);
        }

        public void Initialize(IServiceCollection services, ISettingProvider settings)
        {
        }
    }
}