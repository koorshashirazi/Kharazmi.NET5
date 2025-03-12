namespace Kharazmi.Common.Events
{
    /// <summary>_</summary>
    public class ChannelEvent
    {
        /// <summary>_</summary>
        public ChannelEvent(string domainEvent, string domainEventType, string domainMetadata)
        {
            DomainEvent = domainEvent;
            DomainEventType = domainEventType;
            DomainMetadata = domainMetadata;
        }

        /// <summary>_</summary>
        public string DomainEvent { get; }

        /// <summary>_</summary>
        public string DomainEventType { get; }

        /// <summary>_</summary>
        public string DomainMetadata { get; }
    }
}