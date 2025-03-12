using System.Diagnostics.CodeAnalysis;

namespace Kharazmi.RabbitMq.Extensions
{
    internal static class Constants
    {
        public const string RabbitMq = "#RabbitMq- ";
        public const string Subscriber = "#Subscriber- ";
        public const string Publisher = "#Publisher- ";
        public const string Retry = "#Retry- ";
    }

    /// <summary> </summary>
    public static class PrefixExtensions
    {
        /// <summary> </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string WithRabbitMqPrefix([NotNull] this string value)
            => value.Insert(0, Constants.RabbitMq);

        /// <summary></summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string WithSubscriberPrefix([NotNull] this string value)
            => value.Insert(0, Constants.Subscriber);

        /// <summary></summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string WithPublisherPrefix([NotNull] this string value)
            => value.Insert(0, Constants.Publisher);

        /// <summary></summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string WithRetryPrefix([NotNull] this string value)
            => value.Insert(0, Constants.Retry);
    }
}