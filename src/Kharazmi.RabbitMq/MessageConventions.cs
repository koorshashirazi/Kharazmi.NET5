using Kharazmi.Conventions;

namespace Kharazmi.RabbitMq
{
    internal class MessageConventions : IMessageConventions
    {
        public string RoutingKey { get; }
        public string Exchange { get; }
        public string Queue { get; }

        public MessageConventions(string exchange, string routingKey, string queue)
        {
            Exchange = exchange;
            RoutingKey = routingKey;
            Queue = queue;
        }
    }
}