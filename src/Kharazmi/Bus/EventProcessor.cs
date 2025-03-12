#region

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Common.Events;
using Kharazmi.Common.Metadata;
using Kharazmi.Configuration;
using Kharazmi.Constants;
using Kharazmi.Dependency;
using Kharazmi.Dispatchers;
using Kharazmi.Exceptions;
using Kharazmi.Functional;
using Kharazmi.Options;
using Kharazmi.Outbox;
using Microsoft.Extensions.Logging;

#endregion

namespace Kharazmi.Bus
{
    internal class EventProcessor : IEventProcessor
    {
        private readonly ILogger<EventProcessor>? _logger;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly IInboxOutboxProvider _inboxOutboxProvider;
        private readonly IBusPublisher _busPublisher;
        private readonly ISettingProvider _settings;

        public EventProcessor(
            ServiceFactory<IEventDispatcher> eventDispatcher,
            ServiceFactory<IInboxOutboxProvider> inboxOutboxFactory,
            ServiceFactory<IBusPublisher> busPublisherFactory)
        {
            _logger = eventDispatcher.LoggerFactory.CreateLogger<EventProcessor>();
            _eventDispatcher = eventDispatcher.Instance();
            _inboxOutboxProvider = inboxOutboxFactory.Instance();
            _busPublisher = busPublisherFactory.Instance();
            _settings = eventDispatcher.Settings;
        }

        public async Task<Result> ProcessAsync(
            IEnumerable<IUncommittedEvent>? uncommittedEvents,
            Func<IEnumerable<IUncommittedEvent>, IEnumerable<IDomainEvent>> integrationEventMapper,
            [AllowNull] Func<IEnumerable<IUncommittedEvent>, IEnumerable<IDomainEvent>>? internalEventMapper = null,
            [AllowNull] DomainMetadata? domainMetadata = null,
            CancellationToken token = default)
        {
            try
            {
                _logger?.LogTrace(MessageTemplate.EventProcessorProcessBegin, MessageEventName.EventProcessor,
                    nameof(EventProcessor));

                domainMetadata ??= DomainMetadata.Empty;

                // TODO publish to read store

                if (uncommittedEvents is null)
                    return Result.Ok();

                _logger?.LogTrace(MessageTemplate.EventProcessorRaisingEvent, MessageEventName.EventProcessor,
                    nameof(EventProcessor));

                var aggregateEvents = uncommittedEvents.ToList();
                //  publish to internal subscriber handler
                if (internalEventMapper is not null)
                {
                    var result = await SendInternalDomainEventsAsync(aggregateEvents,
                        domainMetadata, internalEventMapper, token);

                    if (result.Failed)
                        return result;
                }

                _logger?.LogTrace(MessageTemplate.EventProcessorRaisedEvent, MessageEventName.EventProcessor,
                    nameof(EventProcessor));

                _logger?.LogTrace(MessageTemplate.EventProcessorPublishingEvents, MessageEventName.EventProcessor,
                    nameof(EventProcessor));

                return await SendIntegrationDomainEventsAsync(aggregateEvents,
                    domainMetadata, integrationEventMapper, token);
                // TODO publish to Saga
            }
            catch (Exception e)
            {
                _logger?.LogError(MessageTemplate.EventProcessorFailed, MessageEventName.EventProcessor,
                    nameof(EventProcessor), e.Message);
                return Result.Fail("Event processor can't process events");
            }
        }

        private async Task<Result> SendIntegrationDomainEventsAsync(
            IEnumerable<IUncommittedEvent> uncommittedEvents,
            DomainMetadata metadata,
            Func<IEnumerable<IUncommittedEvent>, IEnumerable<IDomainEvent>> mapperHandler,
            CancellationToken token = default)
        {
            var domainEvents = new List<IDomainEvent>();

            domainEvents.AddRange(mapperHandler.Invoke(uncommittedEvents));

            var outBoxOption = _settings.Get<InboxOutboxOption>();
            if (outBoxOption.Enable)
            {
                //  publish to outBox
                return await _inboxOutboxProvider.ProcessMessageAsync(domainEvents, metadata, token: token)
                    .ConfigureAwait(false);
            }

            Result results = Result.Ok();

            foreach (var domainEvent in domainEvents)
            {
                metadata.SetMessageId(domainEvent.MessageId);

                //  publish to external subscriber handler
                var result = await _busPublisher.PublishAsync(domainEvent, metadata, token: token)
                    .ConfigureAwait(false);
                
                if (result.Failed && domainEvent.IsEssentials)
                    throw DomainException.For(result);
                
                results = Result.Combine(result, results);
            }

            return results;
        }

        private async Task<Result> SendInternalDomainEventsAsync(
            IEnumerable<IUncommittedEvent> uncommittedEvents,
            MetadataCollection metadata,
            Func<IEnumerable<IUncommittedEvent>, IEnumerable<IDomainEvent>> mapperHandler,
            CancellationToken token = default)
        {
            var domainEvents = new List<IDomainEvent>();

            domainEvents.AddRange(mapperHandler.Invoke(uncommittedEvents));

            Result results = Result.Ok();
            foreach (IDomainEvent domainEvent in domainEvents)
            {
                var result = await _eventDispatcher.RaiseAsync(domainEvent, domainEvent.GetType(), metadata, token);
                
                if (result.Failed && domainEvent.IsEssentials)
                    throw DomainException.For(result);

                results = Result.Combine(result, results);
            }

            return results;
        }
    }
}