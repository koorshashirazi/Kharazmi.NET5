#region

using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Common.Metadata;
using Kharazmi.Functional;
using Kharazmi.Messages;

#endregion

namespace Kharazmi.Handlers
{
    public interface IDomainNotificationHandler
    {
        Task<Result> HandleAsync(NotificationDomainEvent domainEvent, DomainMetadata domain,
            CancellationToken cancellationToken = default);

        void SetDomainNotification(NotificationDomainEvent notificationDomain);
        NotificationDomainEvent GetDomainNotification();
        bool HasNotification();
        bool IsNotValid();
        void Reset();
    }
}