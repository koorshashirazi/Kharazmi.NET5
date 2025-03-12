namespace Kharazmi.Common.Metadata
{
    public sealed class MetadataKeys
    {
        #region DomainEventMetadata

        public const string EventName = "EventName";
        public const string AggregateType = "AggregateType";
        public const string AggregateId = "AggregateId";
        public const string AggregateName = "AggregateName";
        public const string AggregateVersion = "AggregateVersion";
        public const string OriginalVersion = "OriginalVersion";
        public const string AggregateEventType = "AggregateEventType";
        public const string ExchangeConfiguration = "ExchangeConfiguration";
        public const string ExchangeProperties = "ExchangeProperties";
        public const string ChannelName = "ChannelName";
        public const string ChannelAssemblyOptionsType = "ChannelAssemblyOptionsType";
        public const string ChannelType = "ChannelType";
        public const string ChannelPatternMode = "ChannelPatternMode";
        public const string ChannelCommandFlags = "ChannelCommandFlags";
        public const string ClientId = "ClientId";
        public const string CacheType = "CacheType";
        public const string CacheKey = "CacheKey";
        public const string OccurredOn = "OccurredOn";

        #endregion

        #region DomainContext

        public const string DomainIdHeader = "X-Domain-Id";
        public const string DomainId = "DomainId";
        public const string SpanContext = "SpanContext";
        public const string UserId = "UserId";
        public const string MessageName = "MessageName";
        public const string MessageId = "MessageId";
        public const string MessageType = "MessageType";
        public const string ResourceId = "ResourceId";
        public const string Resource = "Resource";
        public const string Culture = "Culture";
        public const string TraceId = "TraceId";
        public const string ConnectionId = "ConnectionId";
        public const string Origin = "Origin";
        public const string RequestPath = "RequestPath";
        public const string CreatedAt = "CreatedAt";
        public const string UpdatedAt = "UpdatedAt";
        public const string Retries = "Retries";
        public const string BusOptionKey = "BusOptionKey";
        public const string DomainAssemblyName = "DomainAssemblyName";

        #endregion

        #region Environments

        public const string EnvironmentName = "EnvironmentName";
        public const string MachineName = "MachineName";
        public const string ApplicationName = "ApplicationName";
        public const string ApplicationTitle = "ApplicationTitle";
        public const string ApplicationVersion = "ApplicationVersion";

        #endregion Environments
    }
}