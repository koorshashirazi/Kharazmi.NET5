using System;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.BuilderExtensions;
using Kharazmi.Bus;
using Kharazmi.Common.Metadata;
using Kharazmi.Constants;
using Kharazmi.Options.Bus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RawRabbit.Pipe;
using RawRabbit.Pipe.Middleware;

namespace Kharazmi.RabbitMq.Plugins
{
    internal class AvoidDuplicateMessage : StagedMiddleware
    {
        private readonly IServiceProvider _sp;
        private readonly ILogger<AvoidDuplicateMessage> _logger;

        public AvoidDuplicateMessage(IServiceProvider sp)
        {
            _sp = sp;
            _logger = _sp.GetService<ILoggerFactory>()?.CreateLogger<AvoidDuplicateMessage>() ??
                      NullLoggerFactory.Instance.CreateLogger<AvoidDuplicateMessage>();
        }

        public override string StageMarker { get; } = global::RawRabbit.Pipe.StageMarker.MessageDeserialized;

        public override async Task InvokeAsync(IPipeContext context, CancellationToken token = default)
        {
            using var scope = _sp.CreateScope();
            var sp = scope.ServiceProvider;
            var storeOption = sp.GetSettings().Get<BusOption>();
            var store = sp.GetInstance<IBusMessageStore>();
            var messageContext = context.GetMessageContext() as DomainMetadata;
            var messageId = messageContext?.MessageId ?? context.GetDeliveryEventArgs().BasicProperties.MessageId;
            var exchangeName = context.GetDeliveryEventArgs().Exchange;

            _logger.LogTrace(MessageTemplate.ReceivedBusMessageProcessing, MessageEventName.ReceivedBusMessage,
                nameof(AvoidDuplicateMessage), messageId);

            var result = await store.IsSetAsync(messageId, exchangeName, token);
            if (result.Failed)
                _logger.LogError(MessageTemplate.ReceivedBusMessageProcessFailed, MessageEventName.ReceivedBusMessage,
                    nameof(AvoidDuplicateMessage), messageId);

            if (result.Value)
            {
                _logger.LogTrace(MessageTemplate.ReceivedBusMessageExist, MessageEventName.ReceivedBusMessage,
                    nameof(AvoidDuplicateMessage), messageId);
                return;
            }

            await store.TryAddOrUpdateAsync(messageId, exchangeName, storeOption.BusStoreOption?.ExpireAt, token);

            await Next.InvokeAsync(context, token);

            _logger.LogTrace(MessageTemplate.ReceivedBusMessageProcessed, MessageEventName.ReceivedBusMessage,
                nameof(AvoidDuplicateMessage), messageId);
        }
    }
}