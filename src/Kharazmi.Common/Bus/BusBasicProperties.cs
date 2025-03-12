#region

using System.Collections.Generic;

#endregion

namespace Kharazmi.Common.Bus
{
    /// <summary>_</summary>
    public class BusBasicProperties
    {
        /// <summary>_</summary>
        public BusBasicProperties()
        {
        }

        /// <summary>_</summary>
        public string? AppId { get; set; }

        /// <summary>_</summary>
        public string? ClusterId { get; set; }

        /// <summary>_</summary>
        public string? ContentEncoding { get; set; }

        /// <summary>_</summary>
        public string? ContentType { get; set; }

        /// <summary>_</summary>
        public byte DeliveryMode { get; set; }

        /// <summary>_</summary>
        public string? Expiration { get; set; }

        /// <summary>_</summary>
        public IDictionary<string, object>? Headers { get; set; }

        /// <summary>_</summary>
        public string? MessageId { get; set; }

        /// <summary>_</summary>
        public bool Persistent { get; set; }

        /// <summary>_</summary>
        public byte Priority { get; set; }

        /// <summary>_</summary>
        public string? ReplyTo { get; set; }

        /// <summary>_</summary>
        public string? Type { get; set; }

        /// <summary>_</summary>
        public BusPublicationAddress? ReplyToAddress { get; set; }
    }
}