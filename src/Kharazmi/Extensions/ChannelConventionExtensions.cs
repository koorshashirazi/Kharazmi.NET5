using System;
using System.Diagnostics.CodeAnalysis;
using Kharazmi.Common.Events;

namespace Kharazmi.Extensions
{
    /// <summary></summary>
    public static class ChannelConventionExtensions
    {
        /// <summary></summary>
        /// <param name="domainEvent"></param>
        /// <param name="channelPrefix"></param>
        /// <typeparam name="TDomainEvent"></typeparam>
        public static TDomainEvent ChannelName<TDomainEvent>([NotNull] this TDomainEvent domainEvent,
            string? channelPrefix = "") where TDomainEvent : class, IDomainEvent
        {
            if (domainEvent.DomainMessageMetadata.ChannelName.IsNotEmpty()) return domainEvent;
            var channelName = domainEvent.GetType().Name.ToUnderscore();
            if (string.IsNullOrWhiteSpace(channelPrefix))
                channelPrefix = domainEvent.GetType().Namespace?.ToLowerUnderscore();
            domainEvent.DomainMessageMetadata.SetChannelName($"{channelPrefix}_{channelName}".ToLowerInvariant());
            return domainEvent;
        }

        /// <summary></summary>
        /// <param name="domainEvent"></param>
        /// <param name="channelPrefix"></param>
        /// <returns></returns>
        public static string ChannelName([NotNull] this Type domainEvent,
            string? channelPrefix = "")
        {
            if (!typeof(IDomainEvent).IsAssignableFrom(domainEvent)) return "";
            var channelName = domainEvent.Name.ToUnderscore();
            if (string.IsNullOrWhiteSpace(channelPrefix))
                channelPrefix = domainEvent.Namespace?.ToLowerUnderscore();
            return $"{channelPrefix}_{channelName}".ToLowerInvariant();
        }
    }
}