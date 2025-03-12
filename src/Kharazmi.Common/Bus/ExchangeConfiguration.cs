#region

using System.Collections.Generic;

#endregion

namespace Kharazmi.Common.Bus
{
    /// <summary></summary>
    public class ExchangeConfiguration
    {
        /// <summary>_</summary>
        public ExchangeConfiguration()
        {
            AutoDelete = false;
            AutoAck = true;
            NoLocal = true;
            Arguments = new Dictionary<string, string>();
            ExchangeType = Bus.ExchangeType.Topic;
        }
        /// <summary></summary>
        public string? ExchangeName { get; set; }

        /// <summary></summary>
        public string ExchangeType { get; set; }

        /// <summary></summary>
        public string? RoutingKey { get; set; }

        /// <summary></summary>
        public string? QueueName { get; set; }

        /// <summary></summary>
        public string? ConsumerTag { get; set; }

        /// <summary></summary>
        public ushort PrefetchCount { get; set; }

        /// <summary></summary>
        public bool Durability { get; set; }

        /// <summary></summary>
        public bool AutoDelete { get; set; }

        /// <summary></summary>
        public bool AutoAck { get; set; } 

        /// <summary></summary>
        public bool NoLocal { get; set; }

        /// <summary></summary>
        public Dictionary<string, string>? Arguments { get; set; }
    }
}