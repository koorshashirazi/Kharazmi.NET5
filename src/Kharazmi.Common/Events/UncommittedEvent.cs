using Kharazmi.Common.Metadata;
using Newtonsoft.Json;

namespace Kharazmi.Common.Events
{
    /// <summary> </summary>
    public class UncommittedEvent : IUncommittedEvent
    {
        /// <summary> </summary>
        /// <param name="aggregateEvent"></param>
        /// <param name="domainMessageMetadata"></param>
        public UncommittedEvent(IAggregateEvent aggregateEvent, DomainMessageMetadata domainMessageMetadata)
        {
            AggregateEvent = aggregateEvent;
            DomainMessageMetadata = domainMessageMetadata;
        }

        /// <summary></summary>
        [JsonIgnore]
        public IAggregateEvent AggregateEvent { get; }

        /// <summary></summary>
        public DomainMessageMetadata DomainMessageMetadata { get; }
    }
}