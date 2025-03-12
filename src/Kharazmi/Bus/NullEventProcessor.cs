using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Common.Events;
using Kharazmi.Common.Metadata;
using Kharazmi.Dependency;
using Kharazmi.Functional;

namespace Kharazmi.Bus
{
    internal class NullEventProcessor : IEventProcessor, INullInstance
    {
        public Task<Result> ProcessAsync(
            IEnumerable<IUncommittedEvent>? uncommittedEvents, 
            Func<IEnumerable<IUncommittedEvent>, IEnumerable<IDomainEvent>> integrationEventMapper,
            [AllowNull] Func<IEnumerable<IUncommittedEvent>, IEnumerable<IDomainEvent>>? internalEventMapper = null,
            [AllowNull] DomainMetadata? domainMetadata = null,
            CancellationToken token = default)
            => Task.FromResult(Result.Fail("Cant' invoke IDomainEventProcessor service"));
    }
}