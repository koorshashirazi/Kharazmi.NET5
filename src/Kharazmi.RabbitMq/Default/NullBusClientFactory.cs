using Kharazmi.Dependency;
using RawRabbit;

namespace Kharazmi.RabbitMq.Default
{
    internal class NullBusClientFactory : IBusClientFactory, INullInstance
    {
        public IBusClient BusClient { get; } = new NullBusClient();
    }
}