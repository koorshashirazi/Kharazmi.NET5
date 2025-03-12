using System;
using System.Reflection;
using Kharazmi.BuilderExtensions;
using Kharazmi.Bus;
using Kharazmi.Configuration;
using Kharazmi.Constants;
using Kharazmi.Dispatchers;
using Kharazmi.Domain;
using Kharazmi.Extensions;
using Kharazmi.Handlers;
using Kharazmi.Hooks;
using Kharazmi.Metadata;
using Kharazmi.Options.Domain;
using Kharazmi.Pipelines;
using Microsoft.Extensions.DependencyInjection;

namespace Kharazmi.ConfigurePlugins
{
    internal class DomainConfigurePlugin : IConfigurePlugin
    {
        public string PluginName => nameof(DomainConfigurePlugin);

        public void Configure(ISettingProvider settings)
        {
            var option = settings.Get<DomainOption>();

            option.AddOrUpdateCommandPipeline(typeof(LoggerCommandPipeline<>), false);
            option.AddOrUpdateCommandPipeline(typeof(ValidationCommandPipeline<>), false);
            option.AddOrUpdateCommandPipeline(typeof(TransientFaultCommandPipeline<>), false);

            option.AddDomainEventPipeline(typeof(LoggerEventPipeline<>), false);
            option.AddDomainEventPipeline(typeof(TransientFaultEventPipeline<>), false);

            option.AddDomainQueryPipeline(typeof(LoggerQueryPipeline<,>), false);
            option.AddDomainQueryPipeline(typeof(TransientFaultQueryPipeline<,>), false);

            settings.UpdateOption(option);
        }

        public void Initialize(IServiceCollection services, ISettingProvider settings)
        {
            var option = settings.Get<DomainOption>();

            if (option.UseAutoRegisterHandler)
            {
                var assembly = Assembly.GetEntryAssembly();

                try
                {
                    var assemblyHandler = option.AssemblyHandler;
                    if (assemblyHandler.IsNotEmpty())
                        assembly = Assembly.Load(option.AssemblyHandler);

                    if (assembly.IsNull())
                        throw new TypeAccessException(MessageHelper.CanNotLoadAssemblyFromType(
                            MessageEventName.Assembly, "AutoRegisterHandler", assembly?.FullName!));

                    services.RegisterHandlers(new[] {assembly});
                }
                catch
                {
                    throw new TypeAccessException(MessageHelper.CanNotLoadAssemblyFromType(MessageEventName.Assembly,
                        "AutoRegisterHandler", assembly?.FullName!));
                }
            }

            services.AddService<IPipelineLogProvider, DefaultPipelineLogProvider>(ServiceLifetime.Singleton);
            services.AddService<IDomainRetryLogProvider, DefaultDomainRetryLogProvider>(ServiceLifetime.Singleton);
            services.AddService<IBusSubscriberStrategy, DefaultBusSubscriberStrategy>(ServiceLifetime.Transient);
            services.AddService<IDomainNotificationHandler, DomainNotificationHandler>(ServiceLifetime.Scoped);

            services.RegisterWithFactory<IDomainMetadataHook, DomainIdDomainMetadataHook, NullDomainMetadataHook,
                DomainOption>((_, op) => op.UseDomainMetadata && op.DomainMetadataOption.UseDomainId);

            services.RegisterWithFactory<IDomainMetadataHook, AssemblyNameDomainMetadataHook, NullDomainMetadataHook,
                DomainOption>((_, op) => op.UseDomainMetadata && op.DomainMetadataOption.UseAssemblyName);

            services.RegisterWithFactory<IDomainMetadataHook, MachineNameDomainMetadataHook, NullDomainMetadataHook,
                DomainOption>((_, op) => op.UseDomainMetadata && op.DomainMetadataOption.UseMachineName);

            services
                .RegisterWithFactory<IDomainMetadataAccessor, DomainMetadataAccessor, NullDomainMetadataAccessor,
                    DomainOption>(
                    (_, op) => op.UseDomainMetadata);

            services.AddSingleton<CorrelationDomainMetadataMiddleware>();

            services.RegisterWithFactory<ICommandDispatcher, CommandDispatcher, NullDomainDispatcher, DomainOption>(
                (_, op) => op.UseDispatchers);

            services.RegisterWithFactory<IEventDispatcher, EventDispatcher, NullDomainDispatcher, DomainOption>(
                (_, op) => op.UseDispatchers);

            services.RegisterWithFactory<IQueryDispatcher, QueryDispatcher, NullDomainDispatcher, DomainOption>(
                (_, op) => op.UseDispatchers);

            services.RegisterWithFactory<IDomainDispatcher, DomainDispatcher, NullDomainDispatcher, DomainOption>(
                (_, op) => op.UseDispatchers);

            services.RegisterWithFactory<IEventProcessor, EventProcessor, NullEventProcessor, DomainOption>(
                (_, op) => op.UseEventProcessor);

            var options = settings.Get<DomainOption>();

            foreach (var commandPipeline in options.EnabledCommandPipelines())
            {
                if (commandPipeline is null) continue;
                services.TryDecorate(typeof(ICommandHandler<>), commandPipeline);
            }

            foreach (var eventPipeline in options.EnabledEventPipelines())
            {
                if (eventPipeline is null) continue;
                services.TryDecorate(typeof(IEventHandler<>), eventPipeline);
            }

            foreach (var queryPipeline in options.EnabledQueryPipelines())
            {
                if (queryPipeline is null) continue;
                services.TryDecorate(typeof(IQueryHandler<,>), queryPipeline);
            }
        }
    }
}