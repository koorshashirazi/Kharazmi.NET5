#region

using System.Collections.Generic;
using Kharazmi.Common.ValueObjects;
using Newtonsoft.Json;

#endregion

namespace Kharazmi.Common.Events
{
    public interface IDomainEvent : IMessage
    {
        [JsonProperty] string? AggregateStringId { get; }
        [JsonIgnore] IIdentity? AggregateId { get; }
        [JsonIgnore] long AggregateVersion { get; }
        [JsonIgnore] bool IsEssentials { get; }
        IDomainEvent SetAggregateId(IIdentity? value);
        IDomainEvent SetAggregateVersion(long value);
        IDomainEvent UpdateEventMetadata(params KeyValuePair<string, string>[] value);
    }
}