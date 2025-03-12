using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kharazmi.Background;
using Kharazmi.BuilderExtensions;
using Kharazmi.Common.Metadata;
using Kharazmi.Dispatchers;
using Kharazmi.Extensions;
using Kharazmi.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Kharazmi.Jobs
{
    public class RaiseDomainEventJob : IAsyncJob
    {
        public MessageSerialized Message { get; }

        public RaiseDomainEventJob(MessageSerialized message)
        {
            Message = message;
        }

        public Task ExecuteAsync(IServiceProvider provider)
        {
            var dispatcher = provider.GetInstance<IEventDispatcher>();
            var logger = provider.GetService<ILogger<RaiseDomainEventJob>>();

            if (Message.MessageData is null || Message.MessageType is null)
            {
                logger?.LogError(
                    "Invalid domain message data, dispatcher can't send a message with type {EventType}",
                    Message.MessageType);
                return Task.CompletedTask;
            }

            var messageType = Type.GetType(Message.MessageType);
            if (messageType.IsNull())
            {
                logger?.LogError(
                    "Invalid domain message type, dispatcher can't send a message with type {EventType}",
                    Message.MessageType);
                return Task.CompletedTask;
            }

            var domainEvent = Message.MessageData.Deserialize();
            var metadataDic = Message.DomainContextMetadata?.Deserialize<Dictionary<string, string>>();
            var metadata = DomainMetadata.Empty.AddRange(metadataDic);
            return dispatcher.RaiseAsync(domainEvent, messageType, metadata);
        }
    }
}