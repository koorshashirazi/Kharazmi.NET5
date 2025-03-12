using Kharazmi.Bus;
using Kharazmi.Configuration;
using Kharazmi.ConfigurePlugins;
using Kharazmi.Constants;
using Kharazmi.Extensions;
using Kharazmi.Options.Bus;
using Kharazmi.Redis.Channels;
using Kharazmi.Redis.Default;
using Microsoft.Extensions.DependencyInjection;

namespace Kharazmi.Redis.ConfigurePlugins
{
    internal class RedisBusConfigurePlugin : IConfigurePlugin
    {
        public string PluginName => nameof(RedisBusConfigurePlugin);

        public void Configure(ISettingProvider settings)
        {
        }

        public void Initialize(IServiceCollection services, ISettingProvider settings)
        {
            services.RegisterWithFactory<IPubSubFactory, PubSubFactory, NullPubSubFactory,
                BusOption>((_, op) => op.Enable);

            services.RegisterWithFactory<IBusPublisher, RedisBusPublisher, NullBusPublisher,
                BusOption>((_, op) => op.UseBusPublisher && op.DefaultProvider == BusProviderKey.Redis);

            services.RegisterWithFactory<IBusSubscriber, RedisBusSubscriber, NullBusSubscriber,
                BusOption>((_, op) => op.UseSubscriber && op.DefaultProvider == BusProviderKey.Redis);
        }
    }
}