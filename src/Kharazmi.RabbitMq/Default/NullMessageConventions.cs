using Kharazmi.Conventions;
using Kharazmi.Dependency;

namespace Kharazmi.RabbitMq.Default
{
    internal class NullMessageConventions: IMessageConventions, INullInstance
    {
        public string RoutingKey { get; }
        public string Exchange { get; }
        public string Queue { get; }
    }
}