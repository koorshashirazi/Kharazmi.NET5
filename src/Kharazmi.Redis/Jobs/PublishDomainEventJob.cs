using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kharazmi.Background;
using Kharazmi.BuilderExtensions;
using Kharazmi.Common.Events;
using Kharazmi.Common.Metadata;
using Kharazmi.Configuration;
using Kharazmi.Exceptions;
using Kharazmi.Extensions;
using Kharazmi.Functional;
using Kharazmi.Json;
using Kharazmi.Options.Domain;
using Kharazmi.Redis.Extensions;
using Microsoft.Extensions.Logging;
using Polly;
using StackExchange.Redis;

namespace Kharazmi.Redis.Jobs
{
    internal sealed class PublishDomainEventJob : IAsyncJob
    {
        /// <summary>_</summary>
        /// <param name="domainEventData"></param>
        public PublishDomainEventJob(DomainEventSerialized domainEventData)
            => DomainEventData = domainEventData;

        public DomainEventSerialized DomainEventData { get; }
        public RetryOption? RetryOption { get; set; }

        public async Task ExecuteAsync(IServiceProvider provider)
        {
            var logger = provider.GetSafeService<ILogger<PublishDomainEventJob>>();
            var subscriberFactory = provider.GetInstance<IPubSubFactory>();
            await TryExecuteAsync(subscriberFactory, provider.GetSettings(), logger);
        }

        public async Task<Result> TryExecuteAsync(
            IPubSubFactory factory,
            ISettingProvider settings,
            ILogger<PublishDomainEventJob>? logger)
        {
            var pubSubFactory = factory;
            var eventData = DomainEventData.EventData;
            var eventType = DomainEventData.EventType;
            var channelName = DomainEventData.ChannelName;

            if (eventData.IsEmpty() || channelName.IsEmpty())
            {
                logger?.LogTrace("Unable to publish event to channel. Invalid data");
                return Result.Fail("Unable to publish event to channel. Invalid data");
            }

            var currentRetry = 0;
            var retryOption = RetryOption ?? new RetryOption();
            var retryPolicy = Policy<Result>
                .Handle<RedisChannelException>()
                .WaitAndRetryAsync(retryOption.Attempt,
                    _ => TimeSpan.FromSeconds(retryOption.MaxDelay));

            var channelType = Type.GetType(eventType);
            var domainEvent = eventData.Deserialize(channelType);
            var domainMetadata = DomainMetadata.Empty;
            domainMetadata.AddRange(DomainEventData.Metadata.Deserialize<Dictionary<string, string>>());

            if (domainEvent is null)
                return Result.Fail(
                    $"Unable to publish event to channel {channelName} with channel type {eventType}");

            var channelEvent = new ChannelEvent(eventData, eventType, DomainEventData.Metadata).Serialize();
            var redisOptionType = settings.GetRedisOption(DomainEventData.BusOptionKey);

            var result = await retryPolicy.ExecuteAsync(async () =>
            {
                try
                {
                    await pubSubFactory.GetPubSub(redisOptionType)
                        .PublishAsync(channelName, channelEvent, (CommandFlags) DomainEventData.CommandFlags)
                        .ConfigureAwait(false);

                    logger?.LogTrace(
                        "Published event to channel {Channel} with type {EventFullType} and domain id {DomainId}",
                        channelName, eventType, domainMetadata.DomainId);
                    return Result.Ok();
                }
                catch (Exception e)
                {
                    logger?.LogError("{Message}", e.Message);

                    currentRetry++;

                    logger.LogTrace(
                        "Retry to publish a rejected channel event: '{EventFullType}' ", eventType);

                    if (currentRetry < retryOption.Attempt)
                        throw new RedisChannelException("Unable to publish channel event");

                    var failedEvent = new RejectedDomainEvent
                        {
                            Reason = $"Unable to publish event with type {eventType}",
                            DomainContext = domainMetadata.DomainId
                        }
                        .FromDomainEvent(domainEvent);

                    await pubSubFactory.GetPubSub(redisOptionType)
                        .PublishAsync(channelName, failedEvent.Serialize(),
                            (CommandFlags) DomainEventData.CommandFlags)
                        .ConfigureAwait(false);

                    return Result.Fail($"Unable to publish event with type {eventType}");
                }
            });

            return result is null
                ? Result.Ok()
                : Result.Fail(
                    $"Can't publish event to channel {channelName} with type {eventType}");
        }
    }
}