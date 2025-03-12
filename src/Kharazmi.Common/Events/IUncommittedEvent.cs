using Kharazmi.Common.Metadata;
using Newtonsoft.Json;

namespace Kharazmi.Common.Events
{
    /// <summary>_</summary>
    public interface IUncommittedEvent
    {
        /// <summary>_</summary>
        [JsonIgnore]
        IAggregateEvent AggregateEvent { get; }

        /// <summary> _</summary>
        DomainMessageMetadata DomainMessageMetadata { get; }
    }
}