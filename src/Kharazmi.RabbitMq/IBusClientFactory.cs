using Kharazmi.Dependency;
using RawRabbit;

namespace Kharazmi.RabbitMq
{
    public interface IBusClientFactory : IShouldBeSingleton, IMustBeInstance
    {
        IBusClient BusClient { get; }
    }
}