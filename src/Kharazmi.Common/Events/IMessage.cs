#region

using System;
using Kharazmi.Common.Metadata;
using Kharazmi.Common.ValueObjects;
using Newtonsoft.Json;

#endregion

namespace Kharazmi.Common.Events
{
    public interface IMessage
    {
        [JsonIgnore] DateTime OccurredOn { get; }

        [JsonIgnore,] Type MessageType { get; }

        [JsonIgnore] string MessageName { get; }

        [JsonIgnore] DomainMessageMetadata DomainMessageMetadata { get; }

        [JsonProperty] MessageId MessageId { get; }

        [JsonProperty] string MessageAssemblyType { get; }
    }
}