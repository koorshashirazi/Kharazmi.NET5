#region

using System;

#endregion

namespace Kharazmi.Notifications
{
    [Serializable]
    public class NotificationOptions
    {
        public string? NotificationKey { get; }

        public NotificationOptions(string? notificationKey = null)
        {
            NotificationKey = notificationKey;
            Code = Guid.NewGuid().ToString("N");
        }

        public static NotificationOptions Empty => new();

        public static NotificationOptions For(NotificationType type, string message, string title)
        {
            return new()
            {
                Title = title,
                Message = message,
                NotificationType = type
            };
        }

        public string Code { get; set; }
        public string? Title { get; set; }
        public bool ShowNewestOnTop { get; set; }
        public bool ShowCloseButton { get; set; }
        public string? Message { get; set; }
        public NotificationType NotificationType { get; set; }
        public bool IsSticky { get; set; }
    }
}