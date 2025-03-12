using System;
using Kharazmi.BuilderExtensions;
using Kharazmi.Options.RabbitMq;
using Kharazmi.RabbitMq.Plugins;
using RawRabbit;
using RawRabbit.Instantiation;

namespace Kharazmi.RabbitMq.Bus
{
    internal class BusClientFactory : IBusClientFactory
    {
        private readonly IServiceProvider _sp;
        private readonly IConfigurePluginBuilder _builder;
        private IBusClient? _client;

        public BusClientFactory(IServiceProvider sp, IConfigurePluginBuilder builder)
        {
            _sp = sp;
            _builder = builder;
            BusClient = CreateBusClient();
        }

        public IBusClient BusClient { get; }

        private IBusClient CreateBusClient()
        {
            if (_client is not null) return _client;
            _client = RawRabbitFactory.CreateInstanceFactory(new RawRabbitOptions
            {
                DependencyInjection = ioc => { _builder.GetBuilder<RabbitMqBuilder>()?.BuildDependencies(_sp, ioc); },
                Plugins = p =>
                {
                    _builder.GetBuilder<RabbitMqBuilder>()
                        ?.BuildPlugins(_sp.GetSettings().Get<RabbitMqOption>(), p);
                }
            }).Create();

            return _client;
        }
    }
}