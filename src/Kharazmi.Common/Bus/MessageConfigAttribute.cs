#region

using System;

#endregion

namespace Kharazmi.Common.Bus
{
    /// <summary></summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class MessageConfigAttribute : Attribute
    {
        /// <summary>_</summary>
        public MessageConfigAttribute()
        {
            
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
        public bool Durability { get; set; }

        /// <summary></summary>
        public bool AutoDelete { get; set; }
    }
}