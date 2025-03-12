using System;
using System.Threading;
using Kharazmi.Common.Bus;
using Kharazmi.Common.Events;
using Kharazmi.Dependency;

namespace Kharazmi.Bus
{
    public class NullBusSubscriber : IBusSubscriber, INullInstance
    {
        public NullBusSubscriber()
        {
        }

        public IBusSubscriber SubscribeTo<TEvent>(
            Func<TEvent, Exception, IRejectedDomainEvent>? onFailed = null,
            MessageConfiguration? messageConfiguration = null,
            CancellationToken token = default) where TEvent : class, IDomainEvent
            => this;
    }
}