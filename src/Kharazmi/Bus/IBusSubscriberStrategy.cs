using System;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Common.Events;
using Kharazmi.Common.Metadata;
using Kharazmi.Dependency;
using Kharazmi.Functional;
using Kharazmi.Options;

namespace Kharazmi.Bus
{
    public interface IBusSubscriberStrategy : IShouldBeSingleton
    {
        Task BeforeDispatchEventAsync(IDomainEvent? domainEvent, CancellationToken token = default);
        Task OnHandleEventFailedAsync(IDomainEvent? domainEvent, Result result, CancellationToken token = default);
        Task OnHandleEventSucceedAsync(IDomainEvent? domainEvent, CancellationToken token = default);

        Task OnRetryHandleRejectedEventAsync(IDomainEvent? domainEvent, int retryCounter,
            CancellationToken token = default);

        Task OnDispatchEventFailedAsync(IDomainEvent? domainEvent, Exception e,
            DomainMetadata? domainContext = null,
            Func<IDomainEvent, Exception, IRejectedDomainEvent>? onFailed = null,
            IChannelOptions? redisDbOptions = null,
            CancellationToken token = default);
    }
}