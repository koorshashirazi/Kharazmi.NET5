#region

using System.Linq;
using Kharazmi.BuilderExtensions;
using Kharazmi.Bus;
using Kharazmi.Common.Metadata;
using Kharazmi.Constants;
using Kharazmi.Extensions;
using Kharazmi.Options.Bus;
using Kharazmi.Options.RabbitMq;
using Kharazmi.RabbitMq.Bus;
using Kharazmi.RabbitMq.Plugins;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RawRabbit;
using RawRabbit.Common;
using RawRabbit.Configuration;
using RawRabbit.Enrichers.MessageContext;
using RawRabbit.Instantiation;

#endregion

namespace Kharazmi.RabbitMq.Extensions
{
    /// <summary></summary>
    public static class FrameworkBuilderExtensions
    {
        /// <summary>_</summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IConfigurePluginBuilder AddRabbitMqConfigurePlugin(this IConfigurePluginBuilder builder)
        {
            builder.AddConfigurePluginsFrom(new[] {typeof(AssemblyType).Assembly});
            var rabbitMqBuilder = new RabbitMqBuilder(builder);
            rabbitMqBuilder
                .AddDependency((sp, ioc) =>
                {
                    // var conventionFactory = context.GetRequiredService<IMessageConventionFactory>();
                    var settings = sp.GetSettings();
                    var option = settings.Get<RabbitMqOption>();
                    option.Ssl?.Version.MapSslProtocols();
                    option.Ssl?.AcceptablePolicyErrors.MapSslPolicyErrors();
                    var configuration = option.MapTo<RawRabbitConfiguration>();
                    ioc.AddSingleton(sp);
                    ioc.AddSingleton(configuration);
                    ioc.AddSingleton<INamingConventions>(new CustomNamingConventions(settings));
                })
                .AddPlugin((op, p) =>
                {
                    if (op.DefaultPlugins.Any(x => x.Key == RabbitMqDefaultPluginName.UseAttributeRouting && x.Value))
                        p.UseAttributeRouting();
                    if (op.DefaultPlugins.Any(x => x.Key == RabbitMqDefaultPluginName.UseRetryLater && x.Value))
                        p.UseRetryLater();
                    if (op.DefaultPlugins.Any(x => x.Key == RabbitMqDefaultPluginName.UseRetryStrategy && x.Value))
                        p.UseRetryStrategy();
                    if (op.DefaultPlugins.Any(x =>
                        x.Key == RabbitMqDefaultPluginName.UseAvoidDuplicateMessage && x.Value))
                        p.UseAvoidDuplicateMessage();
                    if (op.DefaultPlugins.Any(x => x.Key == RabbitMqDefaultPluginName.UseMessageContext && x.Value))
                        p.UseMessageContext<DomainMetadata>();
                    if (op.DefaultPlugins.Any(x => x.Key == RabbitMqDefaultPluginName.UseContextForwarding && x.Value))
                        p.UseContextForwarding();
                });
            builder.AddBuilder(rabbitMqBuilder, false);
            return builder;
        }

        /// <summary>_</summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IBusSubscriber UseRabbitMqSubscriber(this IApplicationBuilder builder)
        {
            var settings = builder.GetSettings();
            var busOption = settings.Get<BusOption>();
            if (busOption.Enable == false) return new NullBusSubscriber();

            var sub = busOption.UseSubscriber && busOption.DefaultProvider == BusProviderKey.RabbitMq
                ? new RabbitMqBusSubscriber(builder.ApplicationServices)
                : (IBusSubscriber) new NullBusSubscriber();

            var options = settings.Get<RabbitMqOption>();

            if (!options.UseAutoModelBuilder) return sub;

            var logger = builder.ApplicationServices.GetService<ILogger<ModelBuilder>>();
            var option = ModelBuilder.CreateAsync(options, logger);

            if (option.ModelBuilderStrategy == RabbitMqModelBuilderStrategies.Ignore ||
                option.ModelBuilderStrategy == RabbitMqModelBuilderStrategies.IgnoreExchange ||
                option.ModelBuilderStrategy == RabbitMqModelBuilderStrategies.IgnoreQueue)
            {
                settings.UpdateOption(option);
                settings.SaveChanges();
            }

            return sub;
        }

        private static IClientBuilder UseRetryStrategy(this IClientBuilder clientBuilder)
        {
            clientBuilder.Register(c => c.Use<RetryStrategy>());
            return clientBuilder;
        }

        private static IClientBuilder UseAvoidDuplicateMessage(this IClientBuilder clientBuilder)
        {
            clientBuilder.Register(c => c.Use<AvoidDuplicateMessage>());
            return clientBuilder;
        }
    }
}