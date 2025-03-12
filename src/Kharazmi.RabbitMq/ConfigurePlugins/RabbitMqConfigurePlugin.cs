using Kharazmi.Configuration;
using Kharazmi.ConfigurePlugins;
using Kharazmi.Conventions;
using Kharazmi.Extensions;
using Kharazmi.Options.RabbitMq;
using Kharazmi.RabbitMq.Bus;
using Kharazmi.RabbitMq.Default;
using Microsoft.Extensions.DependencyInjection;

namespace Kharazmi.RabbitMq.ConfigurePlugins
{
    internal class RabbitMqConfigurePlugin : IConfigurePlugin
    {
        public string PluginName => nameof(RabbitMqConfigurePlugin);

        public void Configure(ISettingProvider settings)
        {
            var option =  settings.Get<RabbitMqOption>();

            settings.UpdateOption(option);
        }

        public void Initialize(IServiceCollection services, ISettingProvider settings)
        {
            services.RegisterWithFactory<IMessageConventionFactory, MessageConventionFactory,
                NullMessageConventionFactory, RabbitMqOption>((_, op) => op.Enable);

            services.RegisterWithFactory<IBusClientFactory, BusClientFactory, NullBusClientFactory,
                RabbitMqOption>((_, op) => op.Enable);

            services.RegisterWithFactory<IBusHandler, RabbitMqBusHandler, NullBusHandler,
                RabbitMqOption>((_, op) => op.UseBusHandler);
        }
    }
}