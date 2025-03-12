
using Newtonsoft.Json;

namespace Kharazmi.Common.Bus
{
    public class BusPublicationAddress
    {
        /// <summary>
        ///  Creates a new instance of the <see cref="T:RabbitMQ.Client.PublicationAddress" />.
        /// </summary>
        /// <param name="exchangeType">Exchange type.</param>
        /// <param name="exchangeName">Exchange name.</param>
        /// <param name="routingKey">Routing key.</param>
        [JsonConstructor]
        public BusPublicationAddress(string exchangeType, string exchangeName, string routingKey)
        {
            ExchangeType = exchangeType;
            ExchangeName = exchangeName;
            RoutingKey = routingKey;
        }

        /// <summary>Retrieve the exchange name.</summary>
        public string ExchangeName { get; private set; }

        /// <summary>Retrieve the exchange type string.</summary>
        public string ExchangeType { get; private set; }

        /// <summary>Retrieve the routing key.</summary>
        public string RoutingKey { get; private set; }


        public override string ToString()
        {
            return ExchangeType + "://" + ExchangeName + "/" + RoutingKey;
        }
    }
}