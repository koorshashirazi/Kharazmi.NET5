#region

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Common.Events;
using Kharazmi.Common.Metadata;
using Kharazmi.Functional;
using Kharazmi.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

#endregion

namespace Kharazmi.Dispatchers
{
    internal class EventDispatcher : IEventDispatcher
    {
        private readonly IServiceProvider _serviceProvider;

        public EventDispatcher(IServiceProvider serviceProvider)
            => _serviceProvider = serviceProvider;


        public async Task<Result> RaiseAsync(
            object? domainEvent,
            Type eventType,
            MetadataCollection? metadata = null,
            CancellationToken token = default)
        {
            var domainEventObj = domainEvent switch
            {
                JObject => JObject.FromObject(domainEvent).ToObject(eventType),
                IDomainEvent => domainEvent,
                _ => default
            };

            if (domainEventObj is not IDomainEvent @event)
                return Result.Fail("Event dispatcher can't raise domain event, Invalid domain event or a null event");

            var serviceType = typeof(IEventHandler<>).MakeGenericType(eventType);
            var handlers = _serviceProvider.GetServices(serviceType);

            var domainMetadata = DomainMetadata.Empty;
            domainMetadata.AddRange(metadata);

            var eventHandlers = handlers.OfType<dynamic>();

            foreach (var eventHandler in eventHandlers)
            {
                dynamic result = await eventHandler.HandleAsync(@event, domainMetadata, token);

                if (result.Failed && @event.IsEssentials)
                    return result;
            }

            return Result.Ok();
        }
    }
}