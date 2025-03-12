#region

using System.Collections.Generic;
using Kharazmi.Dependency;
using Kharazmi.Notifications;

#endregion

namespace Kharazmi.InMemory
{
    public interface INotificationStore : IMustBeInstance
    {
        void Add(NotificationOptions notificationOptions);
        NotificationOptions Get(NotificationOptions notificationOptions);
        IEnumerable<NotificationOptions> Gets(NotificationOptions notificationOptions);
        void Remove(NotificationOptions notificationOptions);
        void Clear(NotificationOptions notificationOptions);
    }
}