using System;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Common.Events;
using Kharazmi.Common.Metadata;
using Kharazmi.Dependency;
using Kharazmi.Functional;
using Kharazmi.Options;
using Microsoft.Extensions.Logging;

namespace Kharazmi.Bus
{
    internal class DefaultBusSubscriberStrategy : IBusSubscriberStrategy
    {
        private readonly IBusPublisher _busPublisher;
        private readonly ILogger<DefaultBusSubscriberStrategy>? _logger;

        public DefaultBusSubscriberStrategy(
            ServiceFactory<IBusPublisher> factory,
            ILogger<DefaultBusSubscriberStrategy>? logger)
        {
            _busPublisher = factory.Instance();
            _logger = logger;
        }

        public Task BeforeDispatchEventAsync(IDomainEvent? domainEvent, CancellationToken token = default)
        {
            if (domainEvent is null) return Task.CompletedTask;

            _logger?.LogTrace(
                "Handling a channel event with type '{ChannelType}' and event id '{@MessageId}'",
                domainEvent.GetType().Name, domainEvent.MessageId);
            return Task.CompletedTask;
        }

        public Task OnHandleEventFailedAsync(IDomainEvent? domainEvent, Result result,
            CancellationToken token = default)
        {
            if (result.Failed)
            {
                _logger?.LogError("{EventHandlerResult}", result.Description);
            }

            return Task.CompletedTask;
        }

        public Task OnHandleEventSucceedAsync(IDomainEvent? domainEvent, CancellationToken token = default)
        {
            if (domainEvent is null) return Task.CompletedTask;
            _logger?.LogTrace(
                "Handled a channel event with type: '{ChannelType}' with event id '{@MessageId}'",
                domainEvent.GetType().Name, domainEvent.MessageId);
            return Task.CompletedTask;
        }

        public Task OnRetryHandleRejectedEventAsync(IDomainEvent? domainEvent, int retryCounter,
            CancellationToken token = default)
        {
            if (domainEvent is null) return Task.CompletedTask;
            _logger?.LogTrace(
                "Retry to handle a rejected channel event with type '{ChannelType}' and after {RetryCounter}} retries",
                domainEvent.GetType().Name, retryCounter);
            return Task.CompletedTask;
        }

        public Task OnDispatchEventFailedAsync(IDomainEvent? domainEvent, Exception e,
            DomainMetadata? domainContext = null,
            Func<IDomainEvent, Exception, IRejectedDomainEvent>? onFailed = null,
            IChannelOptions? redisDbOptions = null,
            CancellationToken token = default)
        {
            _logger?.LogError("{Message}", e.Message);
            if (domainEvent is null) return Task.CompletedTask;
            return PublishRejectedEventAsync(domainEvent, e, domainContext, onFailed, redisDbOptions, token);
        }

        private async Task PublishRejectedEventAsync(IDomainEvent domainEvent, Exception e,
            DomainMetadata? domainContext,
            Func<IDomainEvent, Exception, IRejectedDomainEvent>? failedEventMapper,
            IChannelOptions? channelOptions, 
            CancellationToken token = default)
        {
            if (channelOptions is null)
            {
                _logger?.LogError(" Invalid Channel options type, Unable to publish a rejected event to bus");
                return;
            }

            failedEventMapper ??= (@event, _) => RejectedDomainEvent
                .Create(
                    $"Unable to handle a event with type '{domainEvent.GetType().Name}' and event id: '{domainEvent.MessageId}'")
                .FromDomainEvent(@event);

            var failedDomainEvent = failedEventMapper(domainEvent, e);
            await _busPublisher.PublishAsync(failedDomainEvent, domainContext, token:  token);

            _logger?.LogTrace(
                "Published a rejected  event with type '{ChannelType}'  with event id: '{@MessageId}'",
                domainEvent.GetType(), domainEvent.MessageId);
        }
    }
}