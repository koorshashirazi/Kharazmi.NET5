#region

using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Common.Metadata;
using Kharazmi.Functional;
using Kharazmi.Messages;

#endregion

namespace Kharazmi.Handlers
{
    internal sealed class DomainNotificationHandler : IDomainNotificationHandler
    {
        private NotificationDomainEvent _notifications;

        public DomainNotificationHandler()
            => _notifications = new NotificationDomainEvent();

        public Task<Result> HandleAsync(NotificationDomainEvent domainEvent, DomainMetadata domain,
            CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
                return Task.FromResult(Result.Fail("Cancellation token is Requested"));

            SetDomainNotification(domainEvent);
            return Task.FromResult(Result.Ok());
        }


        public void SetDomainNotification(NotificationDomainEvent notificationDomain)
            => _notifications = notificationDomain;

        public NotificationDomainEvent GetDomainNotification()
            => _notifications;


        public bool HasNotification()
            => _notifications != null;

        public bool IsNotValid()
            => _notifications.Result?.Failed ?? false;

        public void Reset()
        {
            _notifications.Result?.Dispose();
            SetDomainNotification(new NotificationDomainEvent());
        }
    }
}