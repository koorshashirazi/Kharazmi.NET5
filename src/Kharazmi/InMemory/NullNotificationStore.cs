using System.Collections.Generic;
using System.Linq;
using Kharazmi.Dependency;
using Kharazmi.Notifications;

namespace Kharazmi.InMemory
{
    public class NullNotificationStore : INotificationStore, INullInstance
    {
        public void Add(NotificationOptions notificationOptions)
        {
        }

        public NotificationOptions Get(NotificationOptions notificationOptions)
            => new NotificationOptions();

        public IEnumerable<NotificationOptions> Gets(NotificationOptions notificationOptions)
            => Enumerable.Empty<NotificationOptions>();

        public void Remove(NotificationOptions notificationOptions)
        {
        }

        public void Clear(NotificationOptions notificationOptions)
        {
        }
    }
}