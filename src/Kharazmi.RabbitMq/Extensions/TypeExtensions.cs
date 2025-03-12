#region

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Kharazmi.Common.Bus;
using Kharazmi.Common.Events;
using Kharazmi.Extensions;
using Kharazmi.Guard;
using Kharazmi.Options.RabbitMq;
using ExchangeType = RawRabbit.Configuration.Exchange.ExchangeType;

#endregion

namespace Kharazmi.RabbitMq.Extensions
{
    /// <summary></summary>
    internal static class TypeExtensions
    {
        /// <summary></summary>
        /// <param name="type"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static string GetExchangeName([NotNull] this Type type, RabbitMqOption? options)
        {
            var exchangeName = type.GetMessageConfigAttribute()?.ExchangeName;

            var exchange = exchangeName.IsNotEmpty()
                ? exchangeName
                : options?.ExchangeName.IsNotEmpty() == true
                    ? options.ExchangeName
                    : type.Namespace?.ToUnderscore().ToLowerInvariant();

            exchange = typeof(IRejectedDomainEvent).IsAssignableFrom(type)
                ? CustomNamingConventions.ErrorExchangeNaming(options)
                : exchange;

            return exchange ?? type.Name.ToLowerInvariant();
        }

        /// <summary></summary>
        /// <param name="type"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        public static string GetRoutingKey(this Type type, RabbitMqOption option)
        {
            option.NotNull(nameof(option));
            var routingKey = type.GetMessageConfigAttribute()?.RoutingKey;
            var messageNaming = FindMessageNamingConventions(type, option);
            var routingKeyOptions = messageNaming?.RoutingKey;

            routingKey = routingKey.IsNotEmpty()
                ? routingKey
                : routingKeyOptions.IsNotEmpty()
                    ? routingKeyOptions
                    : $"{type.GetExchangeName(option)}.{type.Name.ToUnderscore()}".ToLowerInvariant();

            return routingKey;
        }


        /// <summary>_</summary>
        /// <param name="type"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        public static string GetQueueName(this Type type, RabbitMqOption option)
        {
            option.NotNull(nameof(option));

            var queueName = type.GetMessageConfigAttribute()?.QueueName;

            var typeNameOption = FindMessageNamingConventions(type, option);

            var queueNameOptions = typeNameOption?.QueueName;

            queueName = queueName.IsNotEmpty()
                ? queueName
                : queueNameOptions.IsNotEmpty()
                    ? queueNameOptions
                    : $"{type.GetExchangeName(option)}.{type.Name.ToUnderscore()}".ToLowerInvariant();

            return queueName;
        }

        /// <summary>_</summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static ExchangeType GetExchangeType(this Type type)
        {
            var exchangeType = type.GetMessageConfigAttribute()?.ExchangeType;
            return exchangeType?.MapExchangeType() ?? ExchangeType.Topic;
        }

        /// <summary>_</summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsDurability(this Type type)
        {
            var durability = type.GetMessageConfigAttribute()?.Durability;
            return durability ?? false;
        }

        /// <summary>_</summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsAutoDelete(this Type type)
        {
            var autoDelete = type.GetMessageConfigAttribute()?.AutoDelete;
            return autoDelete ?? false;
        }

        /// <summary>_</summary>
        /// <param name="type"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        public static MessageNamingConventions? FindMessageNamingConventions(this Type type, RabbitMqOption option)
            => option.MessageNamingConventions.FirstOrDefault(x =>
                string.Equals(x.TypeFullName, type.FullName?.Replace("+", "."), StringComparison.OrdinalIgnoreCase));

        private static MessageConfigAttribute? GetMessageConfigAttribute(this MemberInfo type)
            => type.GetCustomAttribute<MessageConfigAttribute>();

        internal static ExchangeType MapExchangeType(this string exchangeType)
        {
            return exchangeType switch
            {
                Common.Bus.ExchangeType.Unknown => ExchangeType.Unknown,
                Common.Bus.ExchangeType.Direct => ExchangeType.Direct,
                Common.Bus.ExchangeType.Fanout => ExchangeType.Fanout,
                Common.Bus.ExchangeType.Headers => ExchangeType.Headers,
                Common.Bus.ExchangeType.Topic => ExchangeType.Topic,
                _ => ExchangeType.Unknown
            };
        }
    }
}