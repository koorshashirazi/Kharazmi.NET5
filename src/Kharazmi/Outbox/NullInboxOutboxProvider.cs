using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Common.Events;
using Kharazmi.Common.Metadata;
using Kharazmi.Common.ValueObjects;
using Kharazmi.Dependency;
using Kharazmi.Functional;

namespace Kharazmi.Outbox
{
    public class NullInboxOutboxProvider : IInboxOutboxProvider, INullInstance, IMustBeInstance
    {
        public Task<Result> HandleIncomingMessageAsync(MessageId messageId, Func<Task<Result>> eventHandler,
            CancellationToken token = default)
            => Task.FromResult(Result.Fail("OutBoxPublisher is disabled"));

        public Task<Result> ProcessMessageAsync<TMessage>(IEnumerable<TMessage> domainEvents,
            MetadataCollection? metadata = null,
            bool? publishAsynchronous = null, CancellationToken token = default) where TMessage : class, IMessage
            => Task.FromResult(Result.Fail("OutBoxPublisher is disabled"));
    }
}