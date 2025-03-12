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
    public class SendCommandJob : IAsyncJob
    {
        public MessageSerialized Message { get; }

        public SendCommandJob(MessageSerialized message)
            => Message = message;

        public Task ExecuteAsync(IServiceProvider provider)
        {
            var dispatcher = provider.GetInstance<ICommandDispatcher>();
            var logger = provider.GetService<ILogger<SendCommandJob>>();

            if (Message.MessageData is null || Message.MessageType is null)
            {
                logger?.LogError(
                    "Invalid domain message data, dispatcher can't send a message with type {CommandType}",
                    Message.MessageType);
                return Task.CompletedTask;
            }

            var messageType = Type.GetType(Message.MessageType);
            if (messageType.IsNull())
            {
                logger?.LogError(
                    "Invalid domain message type, dispatcher can't send a message with type {CommandType}",
                    Message.MessageType);
                return Task.CompletedTask;
            }

            var command = Message.MessageData.Deserialize(messageType);
            var metadataDic = Message.DomainContextMetadata?.Deserialize<Dictionary<string, string>>();
            var metadata = DomainMetadata.Empty.AddRange(metadataDic);

            return dispatcher.SendAsync(command, messageType, metadata);
        }
    }
}