using Kharazmi.Bus;
using Kharazmi.Configuration;
using Kharazmi.Constants;
using Kharazmi.Extensions;
using Kharazmi.Options.Bus;
using Microsoft.Extensions.DependencyInjection;

namespace Kharazmi.ConfigurePlugins
{
    internal class BusConfigurePlugin : IConfigurePlugin
    {
        public string PluginName => nameof(BusConfigurePlugin);

        public void Configure(ISettingProvider settings)
        {
            var option = settings.Get<BusOption>();

            settings.UpdateOption(option);
        }

        public void Initialize(IServiceCollection services, ISettingProvider settings)
        {
            services.RegisterWithFactory<IBusMessageStore, InMemoryBusMessageStore, NullBusMessageStore, BusOption>(
                (_, op) =>
                    op.UseBusStorage && op.BusStoreOption?.BusMessageStoreType ==
                    BusMessageStoreTypeConstants.HostMemory);

            services.RegisterWithFactory<IBusMessageStore, DistributedBusMessageStore, NullBusMessageStore, BusOption>(
                (_, op) =>
                    op.UseBusStorage && op.BusStoreOption?.BusMessageStoreType ==
                    BusMessageStoreTypeConstants.DistributedCache);
        }
    }
}