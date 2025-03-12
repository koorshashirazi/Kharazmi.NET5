using Kharazmi.Dependency;
using RawRabbit;
using RawRabbit.Instantiation;

namespace Kharazmi.RabbitMq.Default
{
    internal class NullInstanceFactory : IInstanceFactory, INullInstance
    {
        public IBusClient Create()
        {
            return new NullBusClient();
        }
    }
}