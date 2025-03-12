namespace Kharazmi.Constants
{
    public static class MessageTemplate
    {
        public const string DomainDispatcherSendMessage =
            "{EventName}:{CategoryName} Send a domain message was successfully, with messagType: {MessageType}";

        public const string DomainDispatcherSendMessageFailed =
            "{EventName}:{CategoryName} Send a domain message was failed, with messagType: {MessageType}, result {Result}";

        public const string DomainMetadataProcessingBegin =
            "{EventName}:{CategoryName} Processing build domain context for request {RequestPath}";

        public const string DomainMetadataProcessingEnd =
            "{EventName}:{CategoryName} Domain Id processing was completed with a final domain ID {domainId}";

        public const string DomainMetadataDisposing =
            "{EventName}:{CategoryName} Disposing the domain context for this request";

        public const string DomainMetadataMissingDomainIdHeader =
            "{EventName}:{CategoryName} Domain Id header is enforced but no Domain Id was not found in the request headers";

        public const string DomainMetadataCanNotBuildDomainId =
            "{EventName}:{CategoryName} Domain id is enforced and can not be ignored, but can't generate domain id from any domain id provider";

        public const string DomainMetadataNotFoundDomainIdHeader =
            "{EventName}:{CategoryName} Domain Id was not found in the request headers, try to generate new domainId";

        public const string DomainMetadataFoundDomainIdHeader =
            "{EventName}:{CategoryName} Domain Id {DomainId} was found in the request headers";

        public const string DomainMetadataUpdatingTraceId =
            "{EventName}:{CategoryName} Updating the TraceIdentifier value on the HttpContext";

        public const string DomainMetadataCreating =
            "{EventName}:{CategoryName} Creating the domain context for this request";

        public const string DomainMetadataWritingDomainIdToResponseHeader =
            "{EventName}:{CategoryName} Writing domain Id response header {ResponseHeader} with value {DomainId}";

        public const string? DomainMetadataGeneratedDomainIdUsingProvider =
            "{EventName}:{CategoryName} Generated a domain Id {DomainId} using the {Type} provider";

        public const string? DomainMetadataGeneratedDomainIdUsingFunction =
            "{EventName}:{CategoryName} Generated a domain Id {DomainId} using the configured generator function";

        public const string HandleMessage =
            "{EventName}:{CategoryName} Handled message with type {MessageType}, eventMetadata {DomainEventMetadata}, value {Message}";

        public const string NotRegisterService =
            "{EventName}:{CategoryName} Any service with type {ServiceType} is not registered";

        public const string AssemblyTypeLoad =
            "{EventName}:{CategoryName} Can't load assembly from type {AssemblyType}";

        public const string NotFoundServiceTypeInAssembly =
            "{EventName}:{CategoryName} Can't find any type assignable from {ServiceType} in assembly {AssemblyName}";

        public const string NotFoundOption =
            "{EventName}:{CategoryName} Can't find option with type {OptionType}, option key {OptionKey}";

        public const string CanNotCreateInstance =
            "{EventName}:{CategoryName} Can't create a instance of type {TypeName}";

        public const string OptionChanged =
            "{EventName}:{CategoryName} An option with name {OptionName} is changed";

        public const string OptionValidation =
            "{EventName}:{CategoryName} Validate a options with type {OptionType}, Validation results: {ValidationResults}";

        public const string ResolvedService =
            "{EventName}:{CategoryName} Resolved a implementation type with type {ImplementationType} of {ServiceType}";

        public const string ResolveServiceFailed =
            "{EventName}:{CategoryName} Can't resolve any service implementation with type {ImplementationType} of {ServiceType}: {Message}";

        public const string CookieSignOut =
            "{EventName}:{CategoryName} Sign out a principal for {CookieScheme} scheme";

        public const string HealthCheckResult =
            "{EventName}:{CategoryName} {ValidationResult}";

        public const string HealthCheckReport =
            "{EventName}:{CategoryName} {Status}";

        public const string CanNotUseCookieAuthentication =
            "{EventName}:{CategoryName} Framework use authentication cookie but is invalid extended cookie options configured";

        public const string CanNotUseHangfire =
            "{EventName}:{CategoryName} Framework use hangfire but  has not a valid configurations";

        public const string CanNotUseHangfireStorage =
            "{EventName}:{CategoryName} JobStorage Type set to {StorageType} but has not a valid configurations";

        public const string HangfireInvalidStorage =
            "{EventName}:{CategoryName} Invalid JobStorage is selected, Allowed storage type: {StorageTypes}";

        public const string CanNotUseOpenIdConnect =
            "{EventName}:{CategoryName} Framework use openId connect but is invalid extended openId options configured";

        public const string CanNotUseHealthCheck =
            "{EventName}:{CategoryName} Framework use health check but has not a valid configurations";

        public const string CanNotUseHealthCheckUi =
            "{EventName}:{CategoryName} Framework use health check ui but has not a valid configurations";

        public const string CanNotUseMongoDb =
            "{EventName}:{CategoryName} Framework use mongo db but has not a valid configurations";

        public const string CanNotUseRedis =
            "{EventName}:{CategoryName} Framework use mongo db but has not a valid configurations";

        public const string CanNotUseRabbitMq =
            "{EventName}:{CategoryName} Framework use rabbitMq but has not a valid configurations";

        public const string CanNotUseMailServer =
            "{EventName}:{CategoryName} Framework use mail server but has not a valid configurations";

        public const string CanNotMongoDistributedCache =
            "{EventName}:{CategoryName} Framework use mongo distributed cache but has not a valid configurations";

        public const string CanNotRedisDistributedCache =
            "{EventName}:{CategoryName} Framework use redis distributed cache but has not a valid configurations";

        public const string InvalidMongoDbOptions =
            "{EventName}:{CategoryName} Framework use mongo db of the type specified in {MongoOption} but has not a valid configurations";

        public const string InvalidRedisDbOptions =
            "{EventName}:{CategoryName} Framework use redis db of the type specified in {RedisDbOptions} but has not a valid configurations";

        public const string NullOrEmpty =
            "{EventName}:{CategoryName} The {Name} can't accept null or empty value";

        public const string NotFoundValueInCollection =
            "{EventName}:{CategoryName} Can't find any value with key {KeyName} in collection {CollectionName}";

        public const string OutOfRange =
            "{EventName}:{CategoryName} Value with key {KeyName} is out of range, Allowd values: {ValuesRange}";

        public const string MustBeGreaterThanFormat =
            "{EventName}:{CategoryName} The {ValueName} value must be greater than {MaxValue}. Given: {GivenValue}";

        public const string MustBeBetweenFormat =
            "{EventName}:{CategoryName} The {ValueName} value must be between {MinValue} and {MaxValue}. Given: {GivenValue}";

        public const string MustBePositive =
            "{EventName}:{CategoryName} The {ValueName} value must be positive. Given: {GivenValue}";

        public const string MaxAllowedFormat =
            "{EventName}:{CategoryName} The {ValueName} value must be positive. Maximum allowed {MaximumValue}. Given: {GivenValue}";

        public const string JobExecuted =
            "{EventName}:{CategoryName} A job with jobId: {JobId} and jobType: {JobType} is executed";

        public const string SchedulerExecuting =
            "{EventName}:{CategoryName} Scheduler begin to executing a background job";

        public const string SchedulerExecuted =
            "{EventName}:{CategoryName} Scheduler executed a background job successfully";

        public const string SchedulerFailed =
            "{EventName}:{CategoryName} Scheduler can't execute a background job successfully";

        public const string JobExecuteFailed =
            "{EventName}:{CategoryName} A job with jobId: {JobId}, jonType: {JobType} and failedMessage: {Message}} is failed";

        public const string SaveSettingSucceed =
            "{EventName}:{CategoryName} Save settings is succeed";

        public const string SaveSettingFailed =
            "{EventName}:{CategoryName} Save settings is failed, result: {Result}";

        public const string NotFoundFilePath =
            "{EventName}:{CategoryName} Not exsist file with filePath {FilePath}";

        public const string ReceivedBusMessageProcessing =
            "{EventName}:{CategoryName} Received a message bus with messageId: {MessageId} to processing";

        public const string ReceivedBusMessageProcessFailed =
            "{EventName}:{CategoryName} Process a message bus with messageId: {MessageId} is failed";

        public const string ReceivedBusMessageProcessed =
            "{EventName}:{CategoryName} Received a message bus with messageId: {MessageId} to be processed";

        public const string ReceivedBusMessageExist =
            "{EventName}:{CategoryName} A received message bus with messageId: {MessageId} war alread processed";

        public const string SetCacheItem =
            "{EventName}:{CategoryName} A cache item with cachekey {Cachekey} was added";

        public const string RemoveCacheItem =
            "{EventName}:{CategoryName} A cache item with key {Cachekey} was removed";

        public const string RemoveCacheItemFailed =
            "{EventName}:{CategoryName} Remove a cache item with cachekey {Cachekey} was failed";

        public const string SetCacheItemFailed =
            "{EventName}:{CategoryName} Add A cache item with cachekey {Cachekey} was failed";

        public const string GetCacheItemResult =
            "{EventName}:{CategoryName} Get cache items with result: {Result}";

        public const string BeforeMessageProcessing =
            "{EventName}:{CategoryName} Processing a message with type {MessageType}";

        public const string MessageProcessed =
            "{EventName}:{CategoryName} A message with type: {MessageType} and domainId: {DomainId} was processed";

        public const string MessageProcessingFailed =
            "{EventName}:{CategoryName} Process a message with type {MessageType} was failed, result: {Result}";

        public const string MessageProcessSucceeded =
            "{EventName}:{CategoryName} Process a message with type {MessageType} was succeeded";

        public const string MessageProcessException =
            "{EventName}:{CategoryName} Can't process a message with type {MessageType}, exceptionMessage: {ExceptionMessage}";

        public const string DomainRetryBeforeHandleMessage =
            "{EventName}:{CategoryName} Try to processing a message with domainId: {DomainId}";

        public const string DomainRetryAfterHandleMessage =
            "{EventName}:{CategoryName} A message with domainId: {DomainId}, after {TotalSeconds} seconds was processed";

        public const string DomainRetryException =
            "{EventName}:{CategoryName} Can't process a message with domainId: {DomainId}, exceptionMessage: {ExceptionMessage}";

        public const string DomainRetry =
            "{EventName}:{CategoryName} Retry to handle a message with domainId: {DomainId}, exceptionType: {ExceptionType}, exceptionMessage: {ExceptionMessage}, attempt: {Attempt}";

        public const string EventProcessorProcessBegin =
            "{EventName}:{CategoryName} Begin to process events";

        public const string EventProcessorRaisingEvent =
            "{EventName}:{CategoryName} Begin to raise internal events";

        public const string EventProcessorRaisedEvent =
            "{EventName}:{CategoryName} Internal events was raised";

        public const string EventProcessorPublishingEvents =
            "{EventName}:{CategoryName} Begin to publish domain events";

        public const string EventProcessorPublishedEvents =
            "{EventName}:{CategoryName} Domain events was published";

        public const string EventProcessorFailed =
            "{EventName}:{CategoryName} Can't process a events, exceptionMessage: {ExceptionMessage}";

        public const string InboxOutboxAlreadyExistMessage =
            "{EventName}:{CategoryName} A received message with messageId: {MessageId} war alread processed";

        public const string InboxOutboxHandlingMessage =
            "{EventName}:{CategoryName} Handling a received message. messageId: {MessageId}";

        public const string InboxOutboxHandledMessage =
            "{EventName}:{CategoryName} A message with messageId: {MessageId} was handled";

        public const string InboxOutboxHandleMessageFailed =
            "{EventName}:{CategoryName} Handle a incomming message failed. messageId: {MessageId}, result: {Result}";

        public const string InboxOutboxStoreMessageFailed =
            "{EventName}:{CategoryName} Store a inbox message failed. messageId: {MessageId}, result: {Result}";

        public const string InboxOutboxIsDisabled =
            "{EventName}:{CategoryName} InboxOutbox is disabled, incoming messages won't be processed. messageId: {MessageId}";

        public const string InboxOutboxDisableToStorage =
            "{EventName}:{CategoryName} InboxOutbox is disabled, messages won't be saved into the storage. messageId: {MessageId}";

        public const string InboxOutboxEmptyMessageId =
            "{EventName}:{CategoryName} InboxOutBox can't process a incoming messages with empty message id";

        public const string InboxOutboxProcessingOutboxMessages =
            "{EventName}:{CategoryName} Processing domain messages with domainId {DomainId}";

        public const string InboxOutboxProcessedOutboxMessages =
            "{EventName}:{CategoryName} Domain messages with domainId {DomainId} was processed";

        public const string InboxOutboxProcessOutboxMessagesFailed =
            "{EventName}:{CategoryName} Processing domain messages with domainId {DomainId} was failed, result {Result}";

        public const string InboxOutboxJobNoExistMessage =
            "{EventName}:{CategoryName} There are no messages to process yet";

        public const string InboxOutboxJobNullMessage =
            "{EventName}:{CategoryName} Can't process a nullable outboxMessage";

        public const string InboxOutboxJobPendingMessage =
            "{EventName}:{CategoryName} Pending messages to publish, messageCount {MessageCount}";

        public const string InboxOutboxJobPublishMessageFailed =
            "{EventName}:{CategoryName} Publish a message with messageId {MessageId} was failed, result: {Result}";

        public const string InboxOutboxJobPublishedMessage =
            "{EventName}:{CategoryName} Publish a message with messageId {MessageId} was successfully";

        public const string InboxOutboxStoreException =
            "{EventName}:{CategoryName} InboxOutbox store can not complete its operation for some reason, exceptionMessage: {ExceptionMessage}";

        public const string MongoDbInstallerCreatedIndex =
            "{EventName}:{CategoryName} Create a index with indexName {IndexName} for collectionName {CollectionName} was procssed";

        public const string MongoDbInstallerCreatedIndexFailed =
            "{EventName}:{CategoryName} DbInstaller can not complete its operation for some reason, exceptionMessage: {ExceptionMessage}";

        public const string SignalRConnecting =
            "{EventName}:{CategoryName} Connecting a client with, connectionId: {ConnectionId}";

        public const string SignalRConnected =
            "{EventName}:{CategoryName} A client with, connectionId: {ConnectionId} was connected";

        public const string SignalRDisconnecting =
            "{EventName}:{CategoryName} Disconnecting a client with, connectionId: {ConnectionId}";

        public const string SignalRDisConnected =
            "{EventName}:{CategoryName} A client with, connectionId: {ConnectionId} was disconnected";

        public const string SignalRConnectingException =
            "{EventName}:{CategoryName} SignalR hub can not complete its connection for some reason, exceptionMessage: {ExceptionMessage}";

        public const string SignalRDisconnectingException =
            "{EventName}:{CategoryName} SignalR hub can not disconnecte its connection for some reason, exceptionMessage: {ExceptionMessage}";

        public const string SignalRInitializerException =
            "{EventName}:{CategoryName} SignaR initializer hub can not complete its operations for some reason, exceptionMessage: {ExceptionMessage}";

        public const string MongoCacheDisabled =
            "{EventName}:{CategoryName} Use second level cache is disabled for mongoDb";
    }
}