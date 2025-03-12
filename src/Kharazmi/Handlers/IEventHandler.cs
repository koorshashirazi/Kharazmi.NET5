#region

using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Common.Events;
using Kharazmi.Common.Metadata;
using Kharazmi.Functional;

#endregion

namespace Kharazmi.Handlers
{
    public interface IEventHandler<in TEvent> where TEvent : IDomainEvent
    {
        Task<Result> HandleAsync(TEvent @event, DomainMetadata domain,
            CancellationToken token = default);

        Task<Result> HandleAsync(IDomainEvent domainEvent, DomainMetadata domain,
            CancellationToken token = default);
    }
}