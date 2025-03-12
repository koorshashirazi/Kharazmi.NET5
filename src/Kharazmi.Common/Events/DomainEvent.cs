#region

using System.Collections.Generic;
using Kharazmi.Common.Json;
using Kharazmi.Common.ValueObjects;
using Newtonsoft.Json;

#endregion

namespace Kharazmi.Common.Events
{
    // TODO DomainEvent json serializer
    /// <summary></summary>
    public abstract class DomainEvent : Message, IDomainEvent
    {
        private long _aggregateVersion;
        private IIdentity? _aggregateId;

        /// <summary>_</summary>
        protected DomainEvent()
        {
            IsEssentials = true;
        }


        /// <summary>_</summary>
        [JsonIgnore]
        public bool IsEssentials { get; set; }

        /// <summary>_</summary>
        [JsonIgnore]
        public long AggregateVersion => _aggregateVersion;


        [JsonProperty] public string? AggregateStringId { get; set; }

        /// <summary>_</summary>

        [JsonIgnore]
        public IIdentity? AggregateId => _aggregateId;

        /// <summary>_</summary>
        /// <summary>_</summary>
        public IDomainEvent SetAggregateId(IIdentity? value)
        {
            if (value == null) return this;
            _aggregateId = value;
            AggregateStringId = value.ValueAsString();
            DomainMessageMetadata.SetAggregateId(value);

            return this;
        }

        /// <summary>_</summary>
        public IDomainEvent SetAggregateVersion(long value)
        {
            _aggregateVersion = value;
            DomainMessageMetadata.SetAggregateVersion(value);
            return this;
        }

        /// <summary>_</summary>
        public IDomainEvent UpdateEventMetadata(params KeyValuePair<string, string>[] value)
        {
            DomainMessageMetadata.AddOrUpdate(value);
            return this;
        }


        /// <summary>Deserialize domain event json</summary>
        /// <param name="value"></param>
        /// <typeparam name="TDomainEvent"></typeparam>
        /// <returns></returns>
        public static TDomainEvent? FromJson<TDomainEvent>(string value) where TDomainEvent : class
        {
            try
            {
                return value.Deserialize<TDomainEvent>();
            }
            catch
            {
                return value.Deserialize() as TDomainEvent;
            }
        }
    }
}