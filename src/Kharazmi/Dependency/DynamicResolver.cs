using System;
using System.Diagnostics.CodeAnalysis;
using Kharazmi.BuilderExtensions;
using Kharazmi.Configuration;
using Kharazmi.Constants;
using Kharazmi.Exceptions;
using Kharazmi.Extensions;
using Kharazmi.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Kharazmi.Dependency
{
    public interface IDynamicResolver
    {
        TService TryResolver<TService, TImplementation, TReplace, TOptions>(Func<ISettingProvider, TOptions, bool> when)
            where TService : class
            where TImplementation : class, TService, IMustBeInstance
            where TReplace : class, TService, IMustBeInstance
            where TOptions : class, IConfigurePluginOption;

        TService Resolver<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService, IMustBeInstance;
    }

    internal class DynamicResolver : IDynamicResolver
    {
        private readonly IServiceProvider _sp;
        private readonly ILogger<DynamicResolver>? _logger;

        public DynamicResolver(
            IServiceProvider sp,
            [AllowNull] ILogger<DynamicResolver>? logger)
        {
            _sp = sp;
            _logger = logger;
        }

        public TService TryResolver<TService, TImplementation, TReplace, TOptions>(
            Func<ISettingProvider, TOptions, bool> when)
            where TService : class
            where TImplementation : class, TService, IMustBeInstance
            where TReplace : class, TService, IMustBeInstance
            where TOptions : class, IConfigurePluginOption
        {
            try
            {
                var settings = _sp.GetSettings();
                var option = settings.Get<TOptions>();
                var hasRule = when(settings, option);

                if (!hasRule)
                {
                    _logger?.LogTrace(MessageTemplate.ResolvedService, MessageEventName.ServiceProvider,
                        nameof(DynamicResolver), typeof(TReplace).Name, typeof(TService).Name);

                    if (typeof(INullInstance).IsAssignableFrom(typeof(TReplace)))
                    {
                        var nullInstance = _sp.GetImplementationInstance<TService, TReplace>();
                        if (nullInstance.IsNull() == false) return nullInstance;

                        nullInstance = ActivatorUtilities.CreateInstance<TReplace>(_sp);
                        if (!typeof(TReplace).ShouldBeSingleton() && !typeof(TService).ShouldBeSingleton())
                            return nullInstance;

                        _sp.AddImplementation<TService, TReplace>(nullInstance);
                        _logger?.LogTrace(MessageTemplate.ResolvedService, MessageEventName.ServiceProvider,
                            nameof(DynamicResolver), typeof(TImplementation).Name, typeof(TService).Name);
                        return nullInstance;
                    }

                    var replaceInstance = _sp.GetImplementationInstance<TService, TReplace>();
                    if (replaceInstance.IsNull() == false) return replaceInstance;

                    replaceInstance = ActivatorUtilities.CreateInstance<TReplace>(_sp);
                    if (!typeof(TReplace).ShouldBeSingleton() && !typeof(TService).ShouldBeSingleton())
                    {
                        _logger?.LogTrace(MessageTemplate.ResolvedService, MessageEventName.ServiceProvider,
                            nameof(DynamicResolver), typeof(TImplementation).Name, typeof(TService).Name);
                        return replaceInstance;
                    }

                    _sp.ReplaceImplementation<TService, TReplace>(replaceInstance);
                    _logger?.LogTrace(MessageTemplate.ResolvedService, MessageEventName.ServiceProvider,
                        nameof(DynamicResolver), typeof(TImplementation).Name, typeof(TService).Name);
                    return replaceInstance;
                }

                var instance = _sp.GetImplementationInstance<TService, TImplementation>();
                if (instance.IsNull() == false)
                {
                    _logger?.LogTrace(MessageTemplate.ResolvedService, MessageEventName.ServiceProvider,
                        nameof(DynamicResolver), typeof(TImplementation).Name, typeof(TService).Name);
                    return instance;
                }

                instance = ActivatorUtilities.CreateInstance<TImplementation>(_sp);
                if (typeof(TImplementation).ShouldBeSingleton() || typeof(TService).ShouldBeSingleton())
                    _sp.ReplaceImplementation<TService, TImplementation>(instance);

                _logger?.LogTrace(MessageTemplate.ResolvedService, MessageEventName.ServiceProvider,
                    nameof(DynamicResolver), typeof(TImplementation).Name, typeof(TService).Name);
                return instance;
            }
            catch (Exception e)
            {
                _logger?.LogError(MessageTemplate.ResolveServiceFailed, MessageEventName.ServiceProvider,
                    nameof(DynamicResolver), typeof(TImplementation).Name, typeof(TService).Name, e.Message);
                return typeof(TReplace).CreateInstance<TReplace>() ??
                       throw new ServiceResolverException(typeof(TService), typeof(TImplementation));
            }
        }

        public TService Resolver<TService, TImplementation>() where TService : class
            where TImplementation : class, TService, IMustBeInstance
        {
            try
            {
                var instance = _sp.GetImplementationInstance<TService, TImplementation>();
                if (instance.IsNull() == false)
                {
                    _logger?.LogTrace(MessageTemplate.ResolvedService, MessageEventName.ServiceProvider,
                        nameof(DynamicResolver), typeof(TImplementation).Name, typeof(TService).Name);
                    return instance;
                }

                instance = ActivatorUtilities.CreateInstance<TImplementation>(_sp);
                if (typeof(TImplementation).ShouldBeSingleton() || typeof(TService).ShouldBeSingleton())
                    _sp.ReplaceImplementation<TService, TImplementation>(instance);

                _logger?.LogTrace(MessageTemplate.ResolvedService, MessageEventName.ServiceProvider,
                    nameof(DynamicResolver), typeof(TImplementation).Name, typeof(TService).Name);
                return instance;
            }
            catch (Exception e)
            {
                _logger?.LogError(MessageTemplate.ResolveServiceFailed, MessageEventName.ServiceProvider,
                    nameof(DynamicResolver), typeof(TImplementation).Name, typeof(TService).Name, e.Message);
                throw new ServiceResolverException(typeof(TService), typeof(TImplementation));
            }
        }
    }
}