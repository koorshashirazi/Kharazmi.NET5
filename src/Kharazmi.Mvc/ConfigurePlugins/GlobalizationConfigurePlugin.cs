using Kharazmi.Configuration;
using Kharazmi.ConfigurePlugins;
using Kharazmi.Extensions;
using Kharazmi.Mvc.Globalization;
using Kharazmi.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Kharazmi.Mvc.ConfigurePlugins
{
    internal class GlobalizationConfigurePlugin : IConfigurePlugin
    {
        public string PluginName => nameof(GlobalizationConfigurePlugin);

        public void Configure(ISettingProvider settings)
        {
            var option = settings.Get<GlobalizationOption>();

            settings.UpdateOption(option);
        }

        public void Initialize(IServiceCollection services, ISettingProvider settings)
        {
            var op = settings.Get<GlobalizationOption>();
            if (!op.UseResourceManager)
            {
                services.ReplaceService(typeof(IResourceManager<>), typeof(NullResourceManager<>),
                    ServiceLifetime.Singleton);
                return;
            }

            services.ReplaceService(typeof(IResourceManager<>), typeof(ResourceManager<>), ServiceLifetime.Singleton);
        }
    }
}