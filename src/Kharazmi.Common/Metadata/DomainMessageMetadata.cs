using System;
using System.Collections.Generic;
using System.Data;
using Kharazmi.Common.Bus;
using Kharazmi.Common.Channels;
using Kharazmi.Common.Extensions;
using Kharazmi.Common.Json;
using Kharazmi.Common.ValueObjects;
using Newtonsoft.Json;

namespace Kharazmi.Common.Metadata
{
    // TODO DomainEventMetadata json serializer
    public class DomainMessageMetadata : MetadataCollection
    {
        public DomainMessageMetadata()
        {
        }

        [JsonIgnore] public static DomainMessageMetadata Empty => new DomainMessageMetadata();

        [JsonIgnore]
        public Type? MessageType =>
            HasValue(MetadataKeys.MessageType)
                ? GetValue(MetadataKeys.MessageType, Type.GetType)
                : default;

        [JsonIgnore] public IIdentity<string> MessageId => GetValue(MetadataKeys.MessageId, Id<string>.FromString);


        [JsonIgnore]
        public Type? AggregateEventType =>
            HasValue(MetadataKeys.AggregateEventType)
                ? GetValue(MetadataKeys.AggregateEventType, Type.GetType)
                : default;

        [JsonIgnore]
        public string? AggregateName =>
            HasValue(MetadataKeys.AggregateName) ? GetValue(MetadataKeys.AggregateName) : default;

        public Type? AggregateType => HasValue(MetadataKeys.AggregateType)
            ? GetValue(MetadataKeys.AggregateType, Type.GetType)
            : default;

        [JsonIgnore]
        public IIdentity? AggregateId =>
            HasValue(MetadataKeys.AggregateId)
                ? GetValue(MetadataKeys.AggregateId, Id<string>.FromString)
                : default;

        [JsonIgnore]
        public long AggregateVersion =>
            HasValue(MetadataKeys.AggregateVersion)
                ? GetValue(MetadataKeys.AggregateVersion, long.Parse)
                : default;

        [JsonIgnore]
        public long OriginalVersion =>
            HasValue(MetadataKeys.OriginalVersion)
                ? GetValue(MetadataKeys.OriginalVersion, long.Parse)
                : default;


        [JsonIgnore]
        public ExchangeConfiguration? ExchangeConfiguration =>
            HasValue(MetadataKeys.ExchangeConfiguration)
                ? GetValue(MetadataKeys.ExchangeConfiguration, ImmutableJson.Deserialize<ExchangeConfiguration>)
                : default;

        [JsonIgnore]
        public BusBasicProperties? ExchangeProperties =>
            HasValue(MetadataKeys.ExchangeProperties)
                ? GetValue(MetadataKeys.ExchangeProperties,
                    ImmutableJson.Deserialize<BusBasicProperties>)
                : default;

        [JsonIgnore]
        public string? ChannelName => HasValue(MetadataKeys.ChannelName) ? GetValue(MetadataKeys.ChannelName) : default;

        [JsonIgnore]
        public PatternMode ChannelPatternMode =>
            HasValue(MetadataKeys.ChannelPatternMode)
                ? GetValue(MetadataKeys.ChannelPatternMode, x => (PatternMode) int.Parse(x))
                : default;

        [JsonIgnore]
        public CommandFlags ChannelCommandFlags =>
            HasValue(MetadataKeys.ChannelCommandFlags)
                ? GetValue(MetadataKeys.ChannelCommandFlags, x => (CommandFlags) int.Parse(x))
                : default;

        [JsonIgnore]
        public IIdentity<string>? ClientId => HasValue(MetadataKeys.ClientId)
            ? GetValue(MetadataKeys.ClientId, Id<string>.FromString)
            : default;

        [JsonIgnore]
        public DateTimeOffset? OccurredOn => HasValue(MetadataKeys.OccurredOn)
            ? GetValue(MetadataKeys.OccurredOn, DateTimeOffset.Parse)
            : default;

     
        public static DomainMessageMetadata From(DomainMessageMetadata metadata)
        {
            DomainMessageMetadata messageMetadata = Empty;

            if (metadata.OccurredOn.IsNotNull())
                messageMetadata.SetOccurredOn(metadata.OccurredOn.Value);

            if (metadata.AggregateId.IsNotNull())
                messageMetadata.SetAggregateId(metadata.AggregateId);

            if (metadata.AggregateName.IsNotNull())
                messageMetadata.SetAggregateName(metadata.AggregateName);

            if (metadata.AggregateType.IsNotNull())
                messageMetadata.SetAggregateType(metadata.AggregateType);

            if (metadata.AggregateVersion.IsNotNull())
                messageMetadata.SetAggregateVersion(metadata.AggregateVersion);

            if (metadata.ChannelName.IsNotNull())
                messageMetadata.SetChannelName(metadata.ChannelName);

            if (metadata.ClientId.IsNotNull())
                messageMetadata.SetClientId(metadata.ClientId);

            if (metadata.MessageId.IsNotNull())
                messageMetadata.SetMessageId(metadata.MessageId);

            if (metadata.ExchangeConfiguration.IsNotNull())
                messageMetadata.SetExchangeConfiguration(metadata.ExchangeConfiguration);

            if (metadata.ExchangeProperties.IsNotNull())
                messageMetadata.SetExchangeProperties(metadata.ExchangeProperties);

            if (metadata.OriginalVersion.IsNotNull())
                messageMetadata.SetOriginalVersion(metadata.OriginalVersion);

            if (metadata.AggregateEventType.IsNotNull())
                messageMetadata.SetAggregateType(metadata.AggregateEventType);

            if (metadata.ChannelCommandFlags.IsNotNull())
                messageMetadata.SetChannelCommandFlags(metadata.ChannelCommandFlags);

            if (metadata.ChannelPatternMode.IsNotNull())
                messageMetadata.SetChannelPatternMode(metadata.ChannelPatternMode);

            if (metadata.MessageType.IsNotNull())
                messageMetadata.SetMessageType(metadata.MessageType);


            return messageMetadata;
        }

        public DomainMessageMetadata SetMessageId(IIdentity<string>? value)
        {
            if (value == null)
                throw new NoNullAllowedException($"MetadataCollection can't accept null value for {nameof(MessageId)}");
            this[MetadataKeys.MessageId] = value.ValueAsString();
            return this;
        }
        
        public DomainMessageMetadata SetMessageType(Type value)
        {
            if (value?.AssemblyQualifiedName == null)
                throw new NoNullAllowedException(
                    $"MetadataCollection can't accept null value for {nameof(MessageType)}");
            this[MetadataKeys.MessageType] = value.AssemblyQualifiedName;
            return this;
        }
        
        public DomainMessageMetadata SetAggregateType(Type? value)
        {
            if (value?.AssemblyQualifiedName?.IsNotEmpty() == true)
                this[MetadataKeys.AggregateType] = value.AssemblyQualifiedName;
            return this;
        }

        public DomainMessageMetadata SetAggregateId(IIdentity? value)
        {
            if (value == null)
                throw new NoNullAllowedException(
                    $"MetadataCollection can't accept null value for {nameof(AggregateId)}");

            this[MetadataKeys.AggregateId] = value.ValueAsString();
            return this;
        }

        public DomainMessageMetadata SetAggregateVersion(long value)
        {
            if (value < 0)
                throw new NoNullAllowedException(
                    $"MetadataCollection can't accept negative value for {nameof(AggregateVersion)}");

            this[MetadataKeys.AggregateVersion] = value.ToString("D");
            return this;
        }

        public DomainMessageMetadata SetAggregateName(string value)
        {
            this[MetadataKeys.AggregateName] = value;
            return this;
        }

        public DomainMessageMetadata SetAggregateEventType(Type? value)
        {
            if (value?.AssemblyQualifiedName == null)
                throw new NoNullAllowedException(
                    $"MetadataCollection can't accept null value for {nameof(AggregateEventType)}");
            this[MetadataKeys.AggregateEventType] = value.AssemblyQualifiedName;
            return this;
        }

        public DomainMessageMetadata SetExchangeConfiguration(ExchangeConfiguration? value)
        {
            if (value == null)
                throw new NoNullAllowedException(
                    $"MetadataCollection can't accept null value for {nameof(ExchangeConfiguration)}");
            this[MetadataKeys.ExchangeConfiguration] = ImmutableJson.Serialize(value);
            return this;
        }

        public DomainMessageMetadata SetExchangeProperties(BusBasicProperties? value)
        {
            if (value == null)
                throw new NoNullAllowedException(
                    $"MetadataCollection can't accept null value for {nameof(ExchangeProperties)}");
            this[MetadataKeys.ExchangeProperties] = ImmutableJson.Serialize(value);
            return this;
        }

        public DomainMessageMetadata SetChannelName(string value)
        {
            if (value.IsEmpty())
                throw new NoNullAllowedException(
                    $"MetadataCollection can't accept null value for {nameof(ChannelName)}");
            this[MetadataKeys.ChannelName] = value;
            return this;
        }

        public DomainMessageMetadata SetChannelPatternMode(PatternMode value)
        {
            this[MetadataKeys.ChannelPatternMode] = ((int) value).ToString();
            return this;
        }

        public DomainMessageMetadata SetChannelCommandFlags(CommandFlags value)
        {
            this[MetadataKeys.ChannelCommandFlags] = ((int) value).ToString();
            return this;
        }

        public DomainMessageMetadata SetClientId(IIdentity<string>? value)
        {
            if (value == null)
                throw new NoNullAllowedException($"MetadataCollection can't accept null value for {nameof(ClientId)}");
            this[MetadataKeys.CacheKey] = value.Value;
            return this;
        }

        public DomainMessageMetadata SetOriginalVersion(long value)
        {
            if (value < 0)
                throw new NoNullAllowedException(
                    $"MetadataCollection can't accept negative value for {nameof(OriginalVersion)}");

            this[MetadataKeys.OriginalVersion] = value.ToString("D");
            return this;
        }

        public DomainMessageMetadata SetOccurredOn(DateTimeOffset value)
        {
            this[MetadataKeys.OccurredOn] = value.ToString("O");
            return this;
        }

        public DomainMessageMetadata AddOrUpdate(params KeyValuePair<string, string>[] value)
        {
            base.AddOrUpdateRange(value);
            return this;
        }

        public DomainMessageMetadata AddOrUpdate(IEnumerable<KeyValuePair<string, string>> value)
        {
            base.AddOrUpdateRange(value);
            return this;
        }
    }
}