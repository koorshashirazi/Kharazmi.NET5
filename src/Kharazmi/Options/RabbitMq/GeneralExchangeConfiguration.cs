#region

using System.Collections.Generic;
using Kharazmi.Common.Bus;

#endregion

namespace Kharazmi.Options.RabbitMq
{
    public class GeneralExchangeConfiguration
    {
        private readonly HashSet<string> _exchangeTypes;
        public GeneralExchangeConfiguration()
        {
            
            AutoDelete = false;
            Durable = true;
            Type = ExchangeType.Topic;
            _exchangeTypes = new HashSet<string>
            {
                ExchangeType.Unknown,
                ExchangeType.Topic,
                ExchangeType.Direct,
                ExchangeType.Fanout,
                ExchangeType.Headers
            };
        }

        public bool Durable { get; set; }

        public bool AutoDelete { get; set; }

        public string Type { get; set; }

        public IReadOnlyCollection<string> ExchangeTypes => _exchangeTypes;
    }
}