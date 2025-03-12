#region

using System;
using System.Reflection;
using Kharazmi.Common.Metadata;
using Kharazmi.Common.ValueObjects;
using Newtonsoft.Json;

#endregion

namespace Kharazmi.Common.Events
{
    /// <summary>_</summary>
    public abstract class Message : IMessage
    {
        private readonly DomainMessageMetadata _domainMessageMetadata;

        /// <summary>_</summary>
        protected Message()
        {
            var eventId = MessageId.FromType(GetType());
            var now = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, TimeZoneInfo.Utc.Id);

            OccurredOn = now;
            MessageName = GetType().Name;
            MessageId = eventId;
            MessageType = GetType();
            MessageAssemblyType = GetType().AssemblyQualifiedName ??
                                  GetType().FullName ??
                                  Assembly.CreateQualifiedName(GetType().Assembly.GetName().FullName,
                                      GetType().FullName) ??
                                  throw new AggregateException(
                                      $"Can not get the assembly-qualified name of {GetType().Name}");

            _domainMessageMetadata = DomainMessageMetadata.Empty
                .SetMessageId(eventId)
                .SetOccurredOn(now)
                .SetMessageType(GetType());
        }

        [JsonIgnore] public DateTime OccurredOn { get; }
        [JsonIgnore] public Type MessageType { get; }
        [JsonIgnore] public string MessageName { get; }
        [JsonProperty] public string MessageAssemblyType { get; }
        [JsonProperty] public MessageId MessageId { get; }
        [JsonIgnore] public DomainMessageMetadata DomainMessageMetadata => _domainMessageMetadata;
    }
}