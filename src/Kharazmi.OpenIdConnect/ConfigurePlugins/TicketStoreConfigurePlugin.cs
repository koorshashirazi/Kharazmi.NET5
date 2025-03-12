using Kharazmi.Configuration;
using Kharazmi.ConfigurePlugins;
using Kharazmi.Constants;
using Kharazmi.Extensions;
using Kharazmi.OpenIdConnect.Default;
using Kharazmi.OpenIdConnect.SessionStores;
using Kharazmi.Options;
using Kharazmi.Options.Cookie;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;

namespace Kharazmi.OpenIdConnect.ConfigurePlugins
{
    internal class TicketStoreConfigurePlugin : IConfigurePlugin
    {
        public string PluginName { get; }

        public void Configure(ISettingProvider settings)
        {
            TicketStoreOption option = settings.Get<TicketStoreOption>();
            settings.UpdateOption(option);
        }

        public void Initialize(IServiceCollection services, ISettingProvider settings)
        {
            services.RegisterWithFactory<ITicketStore, InMemorySessionStore, NullTicketStore, TicketStoreOption>(
                (st, op) =>
                    op.Enable && op.StorageType == TicketStorageType.InMemory &&
                    st.Get<CacheOption>().UseInMemoryCache);

            services.RegisterWithFactory<ITicketStore, DistributedSessionStore, NullTicketStore,
                TicketStoreOption>((st, op) =>
                op.Enable && op.StorageType == TicketStorageType.Distributed &&
                st.Get<CacheOption>().UseDistributedCache);
        }
    }
}