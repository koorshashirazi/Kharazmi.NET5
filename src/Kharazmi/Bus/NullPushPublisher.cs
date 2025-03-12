using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Common.Events;
using Kharazmi.Common.Metadata;
using Kharazmi.Dependency;
using Kharazmi.Functional;

namespace Kharazmi.Bus
{
    public class NullBusPublisher : IBusPublisher, INullInstance
    {
        public Task<Result> PublishAsync<TEvent>(
            [NotNull] TEvent domainEvent,
            MetadataCollection? metadata = null,
            bool? publishAsynchronous = null,
            CancellationToken token = default) where TEvent : class, IDomainEvent
            => Task.FromResult(Result.Fail("Cant' invoke BusManager service"));

    }
}