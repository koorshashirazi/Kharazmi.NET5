#region

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Kharazmi.Constants;
using Kharazmi.Functional;
using Kharazmi.Json;
using Kharazmi.Models;
using Kharazmi.Notifications;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;

#endregion

namespace Kharazmi.Mvc.Extensions
{
    public static class TempDataNotificationExtension
    {
        private const string NotificationKey = NotificationConstant.DomainNotification;

        private static readonly JsonSerializerSettings Settings = Serializer.DefaultJsonSettings;


        public static void AddNotification(this ITempDataDictionary tempData, NotificationOptions options)
        {
            tempData.AddAlert(options);
        }

        public static void AddNotification(
            this Controller controller, NotificationOptions? options = null)
        {
            options ??= new NotificationOptions();
            controller.TempData.AddAlert(options);
        }

        public static Controller AddSuccessMessage(this Controller controller, MessageResult message)
        {
            controller.AddNotification(new NotificationOptions
            {
                NotificationType = NotificationType.Success,
                Message = message.Description,
                Title = message.Code
            });
            return controller;
        }

        public static Controller AddSuccessMessages(this Controller controller, List<MessageResult> messages)
        {
            foreach (var message in messages)
                controller.AddNotification(new NotificationOptions
                {
                    NotificationType = NotificationType.Success,
                    Message = message.Description,
                    Title = message.Code
                });

            return controller;
        }

        public static Controller AddInfoMessage(this Controller controller, MessageResult message)
        {
            controller.AddNotification(new NotificationOptions
            {
                NotificationType = NotificationType.Info,
                Message = message.Description,
                Title = message.Code
            });
            return controller;
        }

        public static Controller AddInfoMessages(this Controller controller, List<MessageResult> messages)
        {
            foreach (var message in messages)
                controller.AddNotification(new NotificationOptions
                {
                    NotificationType = NotificationType.Info,
                    Message = message.Description,
                    Title = message.Code
                });

            return controller;
        }

        public static Controller AddErrorMessage(this Controller controller, MessageResult message)
        {
            controller.AddNotification(new NotificationOptions
            {
                NotificationType = NotificationType.Error,
                Message = message.Description,
                Title = message.Code
            });
            return controller;
        }

        public static Controller AddErrorMessages(this Controller controller, List<MessageResult> messages)
        {
            foreach (var message in messages)
                controller.AddNotification(new NotificationOptions
                {
                    NotificationType = NotificationType.Error,
                    Message = message.Description,
                    Title = message.Code
                });

            return controller;
        }

        public static Controller AddWarningMessage(this Controller controller, MessageResult message)
        {
            controller.AddNotification(new NotificationOptions
            {
                NotificationType = NotificationType.Warning,
                Message = message.Description,
                Title = message.Code
            });
            return controller;
        }

        public static Controller AddWarningMessages(this Controller controller, List<MessageResult> messages)
        {
            foreach (var message in messages)
                controller.AddNotification(new NotificationOptions
                {
                    NotificationType = NotificationType.Warning,
                    Message = message.Description,
                    Title = message.Code
                });

            return controller;
        }

        public static List<NotificationOptions> GetNotifications([NotNull] this ITempDataDictionary tempData)
        {
            if (tempData is null) throw new ArgumentNullException(nameof(tempData));
            CreateTempData(tempData);
            return DeserializeAlerts(tempData.TryGetValue(NotificationKey).ToString());
        }


        public static void RemoveNotifications(this ITempDataDictionary tempData)
        {
            tempData[NotificationKey] = null;
        }

        private static void AddAlert(this ITempDataDictionary tempData, NotificationOptions alert)
        {
            if (alert is null) throw new ArgumentNullException(nameof(alert));

            var deserializeAlertList = tempData.GetNotifications();
            deserializeAlertList.Add(alert);
            tempData[NotificationKey] = SerializeAlerts(deserializeAlertList);
        }

        private static void CreateTempData(this ITempDataDictionary tempData)
        {
            if (!tempData.ContainsKey(NotificationKey))
                tempData[NotificationKey] = string.Empty;
        }

        private static string SerializeAlerts(List<NotificationOptions> tempData)
        {
            return JsonConvert.SerializeObject(tempData, Settings);
        }

        private static List<NotificationOptions> DeserializeAlerts(string? tempData)
        {
            if (string.IsNullOrWhiteSpace(tempData))
                return new List<NotificationOptions>();

            return JsonConvert.DeserializeObject<List<NotificationOptions>>(tempData, Settings) ??
                   new List<NotificationOptions>();
        }

        public static T? Peek<T>(this ITempDataDictionary tempData, string key) where T : class
        {
            var o = tempData.Peek(key);
            return o is null ? null : JsonConvert.DeserializeObject<T>((string) o);
        }

        public static void Put<T>(this ITempDataDictionary tempData, string key, T value) where T : class
        {
            tempData[key] = JsonConvert.SerializeObject(value);
        }
    }
}