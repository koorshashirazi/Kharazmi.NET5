using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Common.Events;
using Kharazmi.Common.Metadata;
using Kharazmi.Common.ValueObjects;
using Kharazmi.Functional;

namespace Kharazmi.Outbox
{
    public interface IInboxOutboxProvider
    {
        Task<Result> HandleIncomingMessageAsync(
            MessageId messageId,
            Func<Task<Result>> eventHandler, CancellationToken token = default);

        Task<Result> ProcessMessageAsync<TMessage>(
            IEnumerable<TMessage> messages,
            MetadataCollection? metadata = null,
            bool? asynchronous = null,
            CancellationToken token = default) where TMessage : class, IMessage;
    }
}