#region

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Common.Events;
using Kharazmi.Common.Metadata;
using Kharazmi.Dependency;
using Kharazmi.Functional;

#endregion

namespace Kharazmi.Bus
{
    public interface IEventProcessor : IMustBeInstance
    {
        Task<Result> ProcessAsync(
            IEnumerable<IUncommittedEvent>? uncommittedEvents,
            Func<IEnumerable<IUncommittedEvent>, IEnumerable<IDomainEvent>> integrationEventMapper,
            Func<IEnumerable<IUncommittedEvent>, IEnumerable<IDomainEvent>>? internalEventMapper = null,
            [AllowNull] DomainMetadata? domainMetadata = null,
            CancellationToken token = default);
    }
}