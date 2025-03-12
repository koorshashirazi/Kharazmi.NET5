using Kharazmi.Bus;
using Kharazmi.Configuration;
using Kharazmi.ConfigurePlugins;
using Kharazmi.Constants;
using Kharazmi.Extensions;
using Kharazmi.Options.Bus;
using Kharazmi.RabbitMq.Bus;
using Microsoft.Extensions.DependencyInjection;

namespace Kharazmi.RabbitMq.ConfigurePlugins
{
    internal class RabbitMqBusConfigurePlugin : IConfigurePlugin
    {
        public string PluginName => nameof(RabbitMqBusConfigurePlugin);

        public void Configure(ISettingProvider settings)
        {
        }

        public void Initialize(IServiceCollection services, ISettingProvider settings)
        {
            services.RegisterWithFactory<IBusPublisher, RabbitMqBusPublisher, NullBusPublisher,
                BusOption>((_, op) => op.UseBusPublisher && op.DefaultProvider == BusProviderKey.RabbitMq);

            services.RegisterWithFactory<IBusSubscriber, RabbitMqBusSubscriber, NullBusSubscriber,
                BusOption>((_, op) => op.UseSubscriber && op.DefaultProvider == BusProviderKey.RabbitMq);
        }
    }
}