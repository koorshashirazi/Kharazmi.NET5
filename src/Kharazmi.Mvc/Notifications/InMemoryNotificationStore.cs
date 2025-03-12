#region

using System.Collections.Generic;
using System.Linq;
using Kharazmi.Constants;
using Kharazmi.Dependency;
using Kharazmi.Extensions;
using Kharazmi.Http;
using Kharazmi.InMemory;
using Kharazmi.Json;
using Kharazmi.Notifications;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

#endregion

namespace Kharazmi.Mvc.Notifications
{
    internal class InMemoryNotificationStore : INotificationStore
    {
        private readonly object _lock = new();

        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<InMemoryNotificationStore> _logger;
        private readonly IUserContextAccessor _userContext;

        // TODO Trace Logger
        public InMemoryNotificationStore(
            ServiceFactory<IMemoryCache> factory,
            ServiceFactory<IUserContextAccessor> userContextFactory)
        {
            _memoryCache = factory.Instance();
            _logger = factory.LoggerFactory.CreateLogger<InMemoryNotificationStore>();
            _userContext = userContextFactory.Instance();
        }


        public void Add(NotificationOptions notificationOptions)
        {
            var notifications = GetOrCreateAlert(notificationOptions);
            lock (_lock)
            {
                notifications.Add(notificationOptions);
                var key = GetKey(notificationOptions);
                _memoryCache.Set(key, notifications.Serialize());
            }
        }


        public NotificationOptions Get(NotificationOptions notificationOptions)
        {
            var key = GetKey(notificationOptions);
            return _memoryCache.TryGetValue(key, out string value)
                ? DeSerialize(value).FirstOrDefault(x => x.Code == notificationOptions.Code) ??
                  new NotificationOptions()
                : new NotificationOptions();
        }

        public IEnumerable<NotificationOptions> Gets(NotificationOptions notificationOptions)
        {
            var key = GetKey(notificationOptions);
            return _memoryCache.TryGetValue(key, out string value)
                ? DeSerialize(value)
                : new List<NotificationOptions>();
        }

        public void Remove(NotificationOptions notificationOptions)
        {
            var notifications = Gets(notificationOptions).ToList();
            var item = notifications.FirstOrDefault(x => x.Code == notificationOptions.Code);
            if (item != null)
                notifications.Remove(item);

            var key = GetKey(notificationOptions);
            _memoryCache.Remove(key);
            _memoryCache.Set(key, notifications.Serialize());
        }

        public void Clear(NotificationOptions notificationOptions)
        {
            var key = GetKey(notificationOptions);
            _memoryCache.Remove(key);
        }


        private List<NotificationOptions> GetOrCreateAlert(NotificationOptions notificationOptions)
        {
            lock (_lock)
            {
                var key = GetKey(notificationOptions);
                return _memoryCache.TryGetValue(key, out string notifications)
                    ? notifications.Deserialize<List<NotificationOptions>>() ??
                      new List<NotificationOptions>()
                    : new List<NotificationOptions>();
            }
        }


        private List<NotificationOptions> DeSerialize(string notifications)
            => notifications.Deserialize<List<NotificationOptions>>() ?? new List<NotificationOptions>();

        private string GetKey(NotificationOptions notificationOptions)
        {
            return notificationOptions.NotificationKey.IsNotEmpty()
                ? $"{NotificationConstant.DomainNotification}:{notificationOptions.NotificationKey}"
                : _userContext.UserId.HasValue
                    ? $"{NotificationConstant.DomainNotification}:{_userContext.UserId.Value}"
                    : NotificationConstant.DomainNotification;
        }
    }
}