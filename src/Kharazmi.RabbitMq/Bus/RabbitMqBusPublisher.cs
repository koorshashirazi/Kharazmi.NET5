#region

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Bus;
using Kharazmi.Common.Bus;
using Kharazmi.Common.Events;
using Kharazmi.Common.Metadata;
using Kharazmi.Configuration;
using Kharazmi.Dependency;
using Kharazmi.Exceptions;
using Kharazmi.Extensions;
using Kharazmi.Functional;
using Kharazmi.Guard;
using Kharazmi.Helpers;
using Kharazmi.Options.RabbitMq;
using Kharazmi.RabbitMq.Extensions;
using Microsoft.Extensions.Logging;
using Polly;
using RabbitMQ.Client;
using RawRabbit;
using RawRabbit.Enrichers.MessageContext;
using ExchangeType = Kharazmi.Common.Bus.ExchangeType;

#endregion

namespace Kharazmi.RabbitMq.Bus
{
    /// <summary> </summary>
    internal class RabbitMqBusPublisher : IBusPublisher
    {
        private readonly IBusClient _busClient;
        private readonly RabbitMqOption _option;
        private readonly ILogger<RabbitMqBusPublisher>? _logger;
        private readonly List<object> _failedDomainEvents = new();

        private const string PublishEventTemp = "{RabbitMq}{Publisher} Publish a event {Name}";

        private const string PreLogMessage =
            "{RabbitMq}{Publisher} Handling a message with type '{MessageType}' and domain id: '{DomainId}'";

        private const string PostLogMessage =
            "{RabbitMq}{Publisher} Handled a message with type '{MessageType}' and domain id: '{DomainId}'";

        /// <summary> </summary>
        public RabbitMqBusPublisher(
            ISettingProvider settingProvider,
            ServiceFactory<IBusClientFactory> factory,
            ILogger<RabbitMqBusPublisher>? logger)
        {
            _busClient = factory.Instance().BusClient;
            _option = settingProvider.Get<RabbitMqOption>();
            _logger = logger;
        }


        // TODO Publish Asynchronous
        /// <summary>
        /// Publish a event to rabbit message broken
        /// </summary>
        /// <param name="domainEvent"></param>
        /// <param name="metadata"></param>
        /// <param name="publishAsynchronous"></param>
        /// <param name="token"></param>
        /// <typeparam name="TEvent"></typeparam>
        /// <returns></returns>
        public async Task<Result> PublishAsync<TEvent>(
            [NotNull] TEvent domainEvent,
            MetadataCollection? metadata = null,
            bool? publishAsynchronous = null,
            CancellationToken token = default) where TEvent : class, IDomainEvent
        {
            metadata ??= DomainMetadata.Empty;

            var eventMetadata = domainEvent.DomainMessageMetadata;

            if (eventMetadata.ExchangeConfiguration is not null)
            {
                return await PublishToAsync(domainEvent, metadata, eventMetadata.ExchangeConfiguration,
                    eventMetadata.ExchangeProperties, publishAsynchronous, token).ConfigureAwait(false);
            }

            return await TryPublishAsync(domainEvent, metadata, async () =>
            {
                await _busClient
                    .PublishAsync(domainEvent, c => c.UseMessageContext(metadata), token)
                    .ConfigureAwait(false);
            }, publishAsynchronous, token);
        }

        /// <summary></summary>
        /// <param name="event"></param>
        /// <param name="metadata"></param>
        /// <param name="exchangeConfiguration"></param>
        /// <param name="properties"></param>
        /// <param name="publishAsynchronous"></param>
        /// <param name="token"></param>
        /// <typeparam name="TEvent"></typeparam>
        /// <returns></returns>
        private Task<Result> PublishToAsync<TEvent>(
            [NotNull] TEvent @event,
            MetadataCollection? metadata = null,
            ExchangeConfiguration? exchangeConfiguration = null,
            BusBasicProperties? properties = null,
            bool? publishAsynchronous = null,
            CancellationToken token = default) where TEvent : class, IDomainEvent
        {
            _logger?.LogTrace(PublishEventTemp, Extensions.Constants.RabbitMq, Extensions.Constants.Publisher, @event.GetType().Name);

            var props = CreateBasicProperties(@event, properties, metadata);

            return TryPublishAsync(@event, metadata, async () =>
                    await PublishMessageAsync(@event, metadata, exchangeConfiguration, props, token),
                publishAsynchronous,
                token);
        }

        private async Task<Result> TryPublishAsync<TMessage>(
            TMessage message,
            MetadataCollection? metadata,
            Func<Task> handle,
            bool? publishAsynchronous = null,
            CancellationToken token = default) where TMessage : class
        {
            message.NotNull(nameof(message));
            handle.NotNull(nameof(handle));

            var domainMetadata = DomainMetadata.Empty;
            domainMetadata.AddRange(metadata);
            var domainId = domainMetadata.DomainId;
            var attribute = message.GetType().GetCustomAttribute<EventAttribute>();
            var messageName = attribute != null ? attribute.Name : message.GetType().Name;
            var retryOption = _option.RetryOption;
            var retries = retryOption?.Attempt ?? 0;
            var retryInterval = retryOption?.MaxDelay ?? 0;
            var retryPolicy = Policy<Exception?>
                .Handle<MessageBusException>()
                .WaitAndRetryAsync(retries, _ => TimeSpan.FromSeconds(retryInterval));

            var isAsynchronous = publishAsynchronous ?? _option.PublishAsynchronous;
            if (isAsynchronous)
            {
                // TODO
            }

            var exception = await retryPolicy.ExecuteAsync(async () =>
            {
                try
                {
                    _logger?.LogTrace(PreLogMessage, Extensions.Constants.RabbitMq, Extensions.Constants.Publisher, messageName, domainId);

                    token.ThrowIfCancellationRequested();

                    await handle.Invoke();

                    _logger?.LogTrace(PostLogMessage, Extensions.Constants.RabbitMq, Extensions.Constants.Publisher, messageName, domainId);

                    return null;
                }
                catch (Exception e)
                {
                    _logger?.LogError("{Message}", e.Message);
                    _logger?.LogTrace(
                        "{RabbitMq}{Subscriber} Published a rejected event: '{MessageName}' with domain id: '{Id}'",
                        Extensions.Constants.RabbitMq, Extensions.Constants.Subscriber, messageName, domainId);

                    _failedDomainEvents.Add(message);
                    throw new MessageBusException($"Retry to handle a rejected event: '{typeof(TMessage).Name}' ",
                        e);
                }
            });

            return exception is null
                ? Result.Ok()
                : Result.Fail(
                    $"Unable to handled a message: '{messageName}' with domain id: '{domainId}'");
        }

        private Task PublishMessageAsync<TMessage>(
            TMessage message,
            MetadataCollection? metadata = null,
            ExchangeConfiguration? exchangeConfiguration = null,
            Action<IBasicProperties>? properties = null,
            CancellationToken token = default)
        {
            var exchangeName = typeof(TMessage).GetExchangeName(_option);
            var routingKey = typeof(TMessage).GetRoutingKey(_option);
            var exchangeType = typeof(TMessage).GetExchangeType();
            var isDurability = typeof(TMessage).IsDurability();
            var isAutoDelete = typeof(TMessage).IsAutoDelete();
            Dictionary<string, string> arguments = new();

            if (exchangeConfiguration is not null)
            {
                if (exchangeConfiguration.ExchangeName.IsNotEmpty())
                    exchangeName = exchangeConfiguration.ExchangeName;

                if (Enum.GetName(typeof(ExchangeType), exchangeConfiguration.ExchangeType).IsNotEmpty())
                    exchangeType = exchangeConfiguration.ExchangeType.MapExchangeType();

                isDurability = exchangeConfiguration.Durability;
                isAutoDelete = exchangeConfiguration.AutoDelete;

                if (exchangeConfiguration.Arguments != null && exchangeConfiguration.Arguments.Count > 0)
                    arguments = exchangeConfiguration.Arguments;
            }

            return _busClient.PublishAsync(message, context =>
            {
                context.UseMessageContext(metadata).UsePublishConfiguration(publisherBuilder =>
                {
                    publisherBuilder.OnDeclaredExchange(exchangeBuilder =>
                    {
                        exchangeBuilder.WithName(exchangeName).WithType(exchangeType)
                            .WithDurability(isDurability)
                            .WithAutoDelete(isAutoDelete);

                        if (arguments is null || arguments.Count <= 0) return;

                        foreach (var argument in arguments)
                            exchangeBuilder.WithArgument(argument.Key, argument.Value);
                    }).OnExchange(exchangeName).WithRoutingKey(routingKey).WithProperties(properties);
                });
            }, token);
        }

        private static Action<IBasicProperties> CreateBasicProperties<TEvent>(TEvent @event,
            BusBasicProperties? basicProperties, MetadataCollection? metadata) where TEvent : IDomainEvent
        {
            return properties => MapBasicProperties(@event, basicProperties, properties, metadata);
        }

        private static void MapBasicProperties<TEvent>(TEvent @event, BusBasicProperties? basicProperties,
            IBasicProperties? properties, MetadataCollection? metadata) where TEvent : IDomainEvent
        {
            if (properties is null)
                return;
            var domainMetadata = DomainMetadata.Empty;
            domainMetadata.AddRange(metadata);

            properties.MessageId = @event.MessageId;

            properties.CorrelationId = domainMetadata.DomainId.IsEmpty()
                ? Guid.NewGuid().ToString("N")
                : domainMetadata.DomainId;

            if (!string.IsNullOrWhiteSpace(domainMetadata.UserId))
                properties.UserId = domainMetadata.UserId;

            properties.Timestamp = new AmqpTimestamp(DateTimeHelper.DateTimeOffsetUtcNow.ToUnixTimeSeconds());

            if (basicProperties is null) return;

            properties.Expiration = basicProperties.Expiration;
            properties.Persistent = basicProperties.Persistent;
            properties.Priority = basicProperties.Priority;
            properties.Type = basicProperties.Type;
            properties.AppId = basicProperties.AppId;
            properties.ClusterId = basicProperties.ClusterId;
            properties.ContentEncoding = basicProperties.ContentEncoding;
            properties.ContentType = basicProperties.ContentType;
            properties.DeliveryMode = basicProperties.DeliveryMode;
            properties.ReplyTo = basicProperties.ReplyTo;

            var replyToAddress = basicProperties.ReplyToAddress;

            if (replyToAddress != null)
                properties.ReplyToAddress = new PublicationAddress(replyToAddress.ExchangeType,
                    replyToAddress.ExchangeName, replyToAddress.RoutingKey);

            properties.Headers = new Dictionary<string, object>();

            if (basicProperties.Headers is null) return;

            foreach (var (key, value) in basicProperties.Headers)
            {
                if (string.IsNullOrWhiteSpace(key) || value is null) continue;

                properties.Headers.TryAdd(key, value);
            }
        }
    }
}