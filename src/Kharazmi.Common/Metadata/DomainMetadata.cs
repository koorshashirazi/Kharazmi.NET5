using System.Data;
using System.Reflection;
using Kharazmi.Common.Domain;
using Kharazmi.Common.Extensions;
using Kharazmi.Common.ValueObjects;

namespace Kharazmi.Common.Metadata
{
    /// <summary>_</summary>
    public class DomainMetadata : MetadataCollection
    {
        /// <summary>_</summary>
        public DomainMetadata()
        {
        }

        /// <summary>_</summary>
        public static DomainMetadata Empty => new DomainMetadata();

        /// <summary>_</summary>
        public string DomainId => HasValue(MetadataKeys.DomainId)
            ? GetValue(MetadataKeys.DomainId)
            : "";

        /// <summary>_</summary>
        public string? SpanContextAsString => HasValue(MetadataKeys.SpanContext)
            ? GetValue(MetadataKeys.SpanContext)
            : default;

        /// <summary>_</summary>
        public string MachineName => HasValue(MetadataKeys.MachineName)
            ? GetValue(MetadataKeys.MachineName)
            : "";

        /// <summary>_</summary>
        public string DomainAssemblyName => HasValue(MetadataKeys.DomainAssemblyName)
            ? GetValue(MetadataKeys.DomainAssemblyName)
            : "";

        /// <summary>_</summary>
        public string? MessageId => HasValue(MetadataKeys.MessageId)
            ? GetValue(MetadataKeys.MessageId)
            : default;

        /// <summary>_</summary>
        public string? UserId => HasValue(MetadataKeys.UserId)
            ? GetValue(MetadataKeys.UserId)
            : default;

        /// <summary>_</summary>
        public string? ResourceId => HasValue(MetadataKeys.ResourceId)
            ? GetValue(MetadataKeys.ResourceId)
            : default;

        /// <summary>_</summary>
        public string? MessageType => HasValue(MetadataKeys.MessageType)
            ? GetValue(MetadataKeys.MessageType)
            : default;

        /// <summary>_</summary>
        public string? Resource => HasValue(MetadataKeys.Resource)
            ? GetValue(MetadataKeys.Resource)
            : default;

        /// <summary>_</summary>
        public string Culture => GetValue(MetadataKeys.Culture);

        /// <summary>_</summary>
        public string? TraceId => HasValue(MetadataKeys.TraceId)
            ? GetValue(MetadataKeys.TraceId)
            : default;

        /// <summary>_</summary>
        public string? ConnectionId => HasValue(MetadataKeys.ConnectionId)
            ? GetValue(MetadataKeys.ConnectionId)
            : default;

        /// <summary>_</summary>
        public string? Origin => HasValue(MetadataKeys.Origin)
            ? GetValue(MetadataKeys.Origin)
            : default;

        /// <summary>_</summary>

        public string? RequestPath => HasValue(MetadataKeys.RequestPath)
            ? GetValue(MetadataKeys.RequestPath)
            : default;

        /// <summary>_</summary>
        public string CreatedAt => GetValue(MetadataKeys.CreatedAt);

        /// <summary>_</summary>
        public string UpdatedAt => GetValue(MetadataKeys.UpdatedAt);

        /// <summary>_</summary>
        public int Retries => GetValue(MetadataKeys.Retries, int.Parse);

        /// <summary>Get bus pub/sub optionKey</summary>
        public string? BusOptionKey => HasValue(MetadataKeys.BusOptionKey)
            ? GetValue(MetadataKeys.BusOptionKey)
            : default;

        /// <summary>_</summary>
        public DomainMetadata SetDomainId(DomainId value)
        {
            if (value is null) throw new NoNullAllowedException("Domain eventMetadata can't accept a null domain id");
            this[MetadataKeys.DomainId] = value.Value;
            return this;
        }      
        
        /// <summary>_</summary>
        public DomainMetadata SetSpanContext(string value)
        {
            this[MetadataKeys.SpanContext] = value;
            return this;
        }

        /// <summary>_</summary>
        public DomainMetadata SetMachineName(string? value)
        {
            if (value?.IsNotEmpty() == true)
                this[MetadataKeys.MachineName] = value;
            return this;
        }

        /// <summary>_</summary>
        public DomainMetadata SetMessageId(MessageId? value)
        {
            if (value is null) return this;
            this[MetadataKeys.MessageId] = value.Value;
            return this;
        }

        /// <summary>_</summary>
        public DomainMetadata SetUserId(UserId? value)
        {
            if (value is null) return this;
            this[MetadataKeys.UserId] = value.Value;
            return this;
        }

        /// <summary>_</summary>
        public DomainMetadata SetResourceId(Id<string>? value)
        {
            if (value is null) return this;
            this[MetadataKeys.ResourceId] = value.Value;
            return this;
        }

        /// <summary>_</summary>
        public DomainMetadata SetResource(string? value)
        {
            if (value is null) return this;
            this[MetadataKeys.Resource] = value;
            return this;
        }

        /// <summary>_</summary>
        public DomainMetadata SetTraceId(string? value)
        {
            if (value is null) return this;
            this[MetadataKeys.TraceId] = value;
            return this;
        }

        /// <summary>_</summary>
        public DomainMetadata SetConnectionId(string? value)
        {
            if (value is null) return this;
            this[MetadataKeys.ConnectionId] = value;
            return this;
        }

        /// <summary>_</summary>
        public DomainMetadata SetOrigin(string? value)
        {
            if (value is null) return this;
            this[MetadataKeys.Origin] = value;
            return this;
        }

        /// <summary>_</summary>
        public DomainMetadata SetRequestPath(string? value)
        {
            if (value is null) return this;
            this[MetadataKeys.RequestPath] = value;
            return this;
        }

        /// <summary>_</summary>
        public DomainMetadata SetRetry(int value)
        {
            if (value <= 0) return this;
            this[MetadataKeys.Retries] = $"{value}";
            return this;
        }

        /// <summary>_</summary>
        public DomainMetadata UpdateDateTime()
        {
            this[MetadataKeys.UpdatedAt] = DateTimeConstants.UtcNow.ToString("O");
            return this;
        }

        /// <summary>_</summary>
        public DomainMetadata IncreaseRetrying()
        {
            var value = 0;
            if (HasValue(MetadataKeys.Retries))
                value = Retries;

            value += 1;
            this[MetadataKeys.Retries] = $"{value}";
            return this;
        }

        /// <summary>Set Bus pub/sub option key</summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public DomainMetadata SetBusOptionKey(string? value)
        {
            if (value is null) return this;
            this[MetadataKeys.BusOptionKey] = value;
            return this;
        }

        public DomainMetadata SetDomainAssemblyName(AssemblyName? value)
        {
            var name = value?.Name;
            if (string.IsNullOrEmpty(name)) return this;
            this[MetadataKeys.DomainAssemblyName] = name;
            return this;
        }
    }
}