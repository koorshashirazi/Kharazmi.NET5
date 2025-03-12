#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kharazmi.Bus;
using Kharazmi.Common.Bus;
using Kharazmi.Common.Events;

#endregion

namespace Kharazmi.RabbitMq.Extensions
{
    /// <summary>_</summary>
    public static class SubscriptionExtensions
    {
        private static Assembly? _messagesAssembly = Assembly.GetCallingAssembly();

        private static ISet<Type>? _excludedMessages = new HashSet<Type>();

        /// <summary>_</summary>
        /// <param name="subscriber"></param>
        /// <param name="messagesAssembly"></param>
        /// <param name="excludedMessages"></param>
        /// <returns></returns>
        public static IBusSubscriber SubscribeAllMessages(this IBusSubscriber subscriber,
            Assembly? messagesAssembly = null,
            ISet<Type>? excludedMessages = null)
        {
            _messagesAssembly = messagesAssembly;
            _excludedMessages = excludedMessages;
            return subscriber.SubscribeAllEvents();
        }

        private static IBusSubscriber SubscribeAllEvents(this IBusSubscriber subscriber)
        {
            return subscriber.SubscribeAllMessages<IDomainEvent>(nameof(IBusSubscriber.SubscribeTo));
        }

        private static IBusSubscriber SubscribeAllMessages<TMessage>
            (this IBusSubscriber subscriber, string subscribeMethod)
        {
            if (_messagesAssembly is null)
                return subscriber;

            var messageTypes = _messagesAssembly
                .GetTypes()
                .Where(t => t.IsClass && typeof(TMessage).IsAssignableFrom(t));

            if (_excludedMessages != null)
                messageTypes = messageTypes.Where(t => !_excludedMessages.Contains(t));

            messageTypes.ToList().ForEach(mt =>
            {
                var exchangeName = mt.GetCustomAttribute<MessageConfigAttribute>()?.ExchangeName;
                if (!string.IsNullOrEmpty(exchangeName))
                    subscriber.GetType().GetMethod(subscribeMethod)?.MakeGenericMethod(mt)
                        .Invoke(subscriber, new object[] {exchangeName, null!, null!});
            });

            return subscriber;
        }
    }
}