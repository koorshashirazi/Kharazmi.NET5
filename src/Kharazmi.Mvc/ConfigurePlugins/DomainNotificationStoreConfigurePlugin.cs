using Kharazmi.Configuration;
using Kharazmi.ConfigurePlugins;
using Kharazmi.Extensions;
using Kharazmi.InMemory;
using Kharazmi.Mvc.Notifications;
using Kharazmi.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Kharazmi.Mvc.ConfigurePlugins
{
    internal class DomainNotificationStoreConfigurePlugin : IConfigurePlugin
    {
        public string PluginName => nameof(DomainNotificationStoreConfigurePlugin);

        public void Configure(ISettingProvider settings)
        {
            
        }

        public void Initialize(IServiceCollection services, ISettingProvider settings)
        {
            services.RegisterWithFactory<INotificationStore, InMemoryNotificationStore, NullNotificationStore,
                CacheOption>((_, op) => op.UseInMemoryNotificationStore);
        }
    }
}