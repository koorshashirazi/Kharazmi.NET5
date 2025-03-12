using System;
using Kharazmi.BuilderExtensions;
using Kharazmi.Bus;
using Kharazmi.Constants;
using Kharazmi.Options.Bus;
using Kharazmi.Redis.Channels;
using Microsoft.AspNetCore.Builder;

namespace Kharazmi.Redis.Extensions
{
    /// <summary>_</summary>
    public static class FrameworkBuilderExtensions
    {
        public static IConfigurePluginBuilder AddRedisConfigurePlugin(this IConfigurePluginBuilder builder)
        {
            builder.AddConfigurePluginsFrom(new[] {typeof(AssemblyType).Assembly});
            return builder;
        }

        /// <summary>_</summary>
        /// <param name="builder"></param>
        /// <param name="optionKey">Default use DefaultOptionKey</param>
        /// <returns></returns>
        public static IBusSubscriber UseRedisSubscriber(this IApplicationBuilder builder, string? optionKey = "")
            => UseRedisSubscriber(builder.ApplicationServices, optionKey);

        /// <summary>_</summary>
        /// <param name="sp"></param>
        /// <param name="optionKey"></param>
        /// <returns></returns>
        public static IBusSubscriber UseRedisSubscriber(this IServiceProvider sp, string? optionKey = "")
        {
            var busOption = sp.GetSettings().Get<BusOption>();
            if (busOption.Enable == false) return new NullBusSubscriber();

            return busOption.UseSubscriber && busOption.DefaultProvider == BusProviderKey.Redis
                ? new RedisBusSubscriber(sp).SubscriberFor(optionKey)
                : new NullBusSubscriber();
        }
    }
}