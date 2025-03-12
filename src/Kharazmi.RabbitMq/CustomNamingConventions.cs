#region

using System;
using Kharazmi.Configuration;
using Kharazmi.Options.RabbitMq;
using Kharazmi.RabbitMq.Extensions;
using RawRabbit.Common;

#endregion

namespace Kharazmi.RabbitMq
{
    internal sealed class CustomNamingConventions : NamingConventions
    {
        public CustomNamingConventions(ISettingProvider settingProvider)
        {
            var options = settingProvider.Get<RabbitMqOption>();
            ExchangeNamingConvention = type => type.GetExchangeName(options);
            RoutingKeyConvention = type => type.GetRoutingKey(options);
            QueueNamingConvention = type => type.GetQueueName(options);
            ErrorExchangeNamingConvention = () => ErrorExchangeNaming(options);
            RetryLaterExchangeConvention = span => RetryLaterExchange(span, options);
            RetryLaterQueueNameConvetion = RetryLaterQueueName;
        }

        private static string RetryLaterQueueName(string exchange, TimeSpan span)
            => $"{exchange.Replace(".", "_")}.retry{span.TotalMilliseconds}_ms"
                .ToLowerInvariant();

        private static string RetryLaterExchange(TimeSpan span, RabbitMqOption option)
            => $"{option.ExchangeName}.retry{span.TotalMilliseconds}_ms".ToLowerInvariant();

        internal static string ErrorExchangeNaming(RabbitMqOption? options)
            => $"{options?.ExchangeName}.error".ToLowerInvariant();
    }
}