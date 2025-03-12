#region

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Kharazmi.BuilderExtensions;
using Kharazmi.Configuration;
using Kharazmi.Dependency;
using Kharazmi.Exceptions;
using Kharazmi.Functional;
using Kharazmi.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;

#endregion

namespace Kharazmi.Extensions
{
    public static partial class ServiceCollectionsExtensions
    {
        private static readonly MethodInfo GetServiceMethod;

        static ServiceCollectionsExtensions()
        {
            Func<IServiceProvider, object?> getServiceMethod = ServiceProviderServiceExtensions.GetService<object>;
            GetServiceMethod = getServiceMethod.Method.GetGenericMethodDefinition();
        }

        public static TService? GetImplementationOf<TService>(this IServiceProvider sp)
        {
            var implementationInstance = sp.GetDynamicServiceProvider().ServiceCollection
                .FirstOrDefault(x => x.ServiceType is TService)?.ImplementationInstance;

            if (implementationInstance is TService instance)
                return instance;

            return default;
        }

        public static object? GetImplementationInstance(this IServiceProvider sp, Type serviceType, Type implementation)
        {
            if (implementation.IsAbstract || !implementation.IsClass) return default;
            var implementationInstance = sp.GetDynamicServiceProvider().ServiceCollection
                .Where(x => x.ServiceType == serviceType)
                .FirstOrDefault(x => x.ImplementationInstance?.GetType() == implementation)?.ImplementationInstance;

            return implementationInstance;
        }

        public static TImplementation? GetImplementationInstance<TService, TImplementation>(this IServiceProvider sp)
            where TService : class
            where TImplementation : class, TService
        {
            var implementationInstance = sp.GetDynamicServiceProvider().ServiceCollection
                .Where(x => x.ServiceType == typeof(TService))
                .FirstOrDefault(x => x.ImplementationInstance is TImplementation)?.ImplementationInstance;

            if (implementationInstance is TImplementation instance)
                return instance;

            return default;
        }

        public static IServiceProvider AddImplementation<TService, TImplementation>(this IServiceProvider sp,
            TImplementation instance) where TService : class where TImplementation : class, TService
        {
            sp.GetDynamicServiceProvider().ServiceCollection.Add(new ServiceDescriptor(typeof(TService), instance));
            return sp;
        }

        public static IServiceProvider ReplaceImplementation<TService, TImplementation>(this IServiceProvider sp,
            TImplementation instance) where TService : class where TImplementation : class, TService
        {
            sp.GetDynamicServiceProvider().ServiceCollection.Replace(new ServiceDescriptor(typeof(TService), instance));
            return sp;
        }

        public static IServiceProvider ReplaceImplementation(this IServiceProvider sp, Type service,
            object instance)
        {
            if (!service.IsInstanceOfType(instance))
                throw new InvalidCastException($"Type of {instance.GetType().Name} is not assassin to {service.Name}");

            sp.GetDynamicServiceProvider().ServiceCollection.Replace(new ServiceDescriptor(service, instance));
            return sp;
        }

        /// <summary>_</summary>
        /// <param name="services"></param>
        /// <param name="serviceLifetime"></param>
        /// <typeparam name="TImplementation"></typeparam>
        /// <typeparam name="TService"></typeparam>
        public static void AddService<TService, TImplementation>(this IServiceCollection services,
            ServiceLifetime serviceLifetime)
            where TImplementation : class, TService
        {
            services.Add(new ServiceDescriptor(typeof(TService), typeof(TImplementation), serviceLifetime));
        }

        public static void AddService(this IServiceCollection services,
            Type serviceType,
            Type implementationsType,
            ServiceLifetime serviceLifetime)
        {
            services.Add(new ServiceDescriptor(serviceType, implementationsType, serviceLifetime));
        }

        public static void AddService(this IServiceCollection services,
            Type serviceType,
            Func<IServiceProvider, object> implementationFactory,
            ServiceLifetime serviceLifetime)
        {
            services.Add(new ServiceDescriptor(serviceType, implementationFactory, serviceLifetime));
        }

        public static void AddService<TService>(this IServiceCollection services,
            Func<IServiceProvider, TService> implementationFactory,
            ServiceLifetime serviceLifetime)
            where TService : class
        {
            services.Add(new ServiceDescriptor(typeof(TService), implementationFactory, serviceLifetime));
        }

        public static void TryAddService<TService, TImplementation>(this IServiceCollection services,
            ServiceLifetime serviceLifetime)
            where TService : class
            where TImplementation : class, TService
        {
            services.TryAdd(new ServiceDescriptor(typeof(TService), typeof(TImplementation), serviceLifetime));
        }

        public static void TryAddService(this IServiceCollection services,
            Type serviceType,
            Type implementationsType,
            ServiceLifetime serviceLifetime)
        {
            if (!serviceType.IsAssignableFrom(implementationsType))
                throw new InvalidCastException(
                    $"Type of {implementationsType.Name} is not assignable from {serviceType.Name}");

            services.TryAdd(new ServiceDescriptor(serviceType, implementationsType, serviceLifetime));
        }

        public static void TryAddService<TService, TImplementation>(this IServiceCollection services,
            Func<IServiceProvider, TImplementation> implementationFactory,
            ServiceLifetime serviceLifetime)
            where TImplementation : class, TService
        {
            services.TryAdd(new ServiceDescriptor(typeof(TService), implementationFactory, serviceLifetime));
        }

        public static void TryAddService<TService>(this IServiceCollection services,
            Func<IServiceProvider, TService> implementationFactory,
            ServiceLifetime serviceLifetime)
            where TService : class
        {
            services.TryAdd(new ServiceDescriptor(typeof(TService), implementationFactory, serviceLifetime));
        }


        public static void ReplaceService<TService, TRequest>(this IServiceCollection services,
            ServiceLifetime serviceLifetime)
            where TRequest : class, TService
        {
            services.Replace(new ServiceDescriptor(typeof(TService), typeof(TRequest), serviceLifetime));
        }

        public static void ReplaceService<TService>(this IServiceCollection services,
            ServiceLifetime serviceLifetime)
            where TService : class
        {
            services.Replace(new ServiceDescriptor(typeof(TService), typeof(TService), serviceLifetime));
        }

        public static void ReplaceService(this IServiceCollection services,
            Type serviceType,
            Type implementationsType,
            ServiceLifetime serviceLifetime)
        {
            services.Replace(new ServiceDescriptor(serviceType, implementationsType, serviceLifetime));
        }

        public static void ReplaceService<TService>(this IServiceCollection services,
            Func<IServiceProvider, TService> implementationFactory,
            ServiceLifetime serviceLifetime)
            where TService : class
        {
            services.Replace(new ServiceDescriptor(typeof(TService), implementationFactory, serviceLifetime));
        }

        public static void ReplaceService(this IServiceCollection services,
            Type serviceType,
            Func<IServiceProvider, Type, object> factory,
            ServiceLifetime serviceLifetime)
        {
            services.Replace(new ServiceDescriptor(serviceType, sp => factory(sp, serviceType), serviceLifetime));
        }

        public static void ReplaceService(this IServiceCollection services,
            Type serviceType, ServiceLifetime serviceLifetime)
        {
            services.Replace(new ServiceDescriptor(serviceType, serviceType, serviceLifetime));
        }


        public static IServiceCollection RegisterWithFactory<TService, TImplementation, TReplace, TOptions>(
            this IServiceCollection services,
            Func<ISettingProvider, TOptions, bool> when,
            ServiceLifetime serviceLifetime = ServiceLifetime.Transient,
            ServiceRegisterStrategy registerStrategy = ServiceRegisterStrategy.Add)
            where TService : class
            where TImplementation : class, TService, IMustBeInstance
            where TReplace : class, TService, IMustBeInstance, INullInstance
            where TOptions : class, IConfigurePluginOption
        {
            static TService ImplementationFactory(IServiceProvider sp, Func<ISettingProvider, TOptions, bool> when)
                => sp.GetDynamicResolver().TryResolver<TService, TImplementation, TReplace, TOptions>(when);


            switch (registerStrategy)
            {
                case ServiceRegisterStrategy.Ignore:
                    break;
                case ServiceRegisterStrategy.ReplaceDefault:
                    services.ReplaceService<TService, TReplace>(serviceLifetime);
                    break;
                case ServiceRegisterStrategy.Add:
                    var serviceInstances = new List<ServiceDescriptor>();
                    var serviceDescriptors = services.Where(x => typeof(TService) == x.ServiceType);

                    serviceInstances.AddRange(serviceDescriptors);
                    var instancesOfService = serviceInstances.Where(x =>
                        x.ImplementationType is not null &&
                        typeof(IMustBeInstance).IsAssignableFrom(x.ImplementationType) == false);

                    foreach (var descriptor in instancesOfService)
                        services.Remove(descriptor);

                    services.AddService(sp => ImplementationFactory(sp, when), serviceLifetime);

                    var nullInstances = services.Where(x =>
                        typeof(INullInstance).IsAssignableFrom(x.ImplementationType)).ToList();

                    var list = new List<ServiceDescriptor>();
                    list.AddRange(nullInstances);

                    var nullInstanceOfService = list.Where(x => x.ServiceType == typeof(TService)).ToList();
                    if (nullInstanceOfService.Count > 1)
                    {
                        nullInstanceOfService.RemoveAt(0);
                        foreach (var descriptor in nullInstanceOfService)
                            services.Remove(descriptor);
                    }

                    break;
                case ServiceRegisterStrategy.Replace:
                    services.ReplaceService(sp => ImplementationFactory(sp, when), serviceLifetime);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(registerStrategy), registerStrategy, null);
            }

            services.TryAddSingleton<Func<IEnumerable<TService>>>(x => x.GetServices<TService>);
            services.TryAddSingleton<ServiceFactory<TService>>();

            return services;
        }

        public static IServiceCollection RegisterWithFactory<TService, TImplementation>(
            this IServiceCollection services,
            ServiceLifetime serviceLifetime = ServiceLifetime.Transient,
            ServiceRegisterStrategy registerStrategy = ServiceRegisterStrategy.Add)
            where TService : class
            where TImplementation : class, TService, IMustBeInstance
        {
            static TService ImplementationFactory(IServiceProvider sp)
                => sp.GetDynamicResolver().Resolver<TService, TImplementation>();


            switch (registerStrategy)
            {
                case ServiceRegisterStrategy.Ignore:
                    break;
                case ServiceRegisterStrategy.ReplaceDefault:
                    services.ReplaceService<TService, TImplementation>(serviceLifetime);
                    break;
                case ServiceRegisterStrategy.Add:
                    var serviceInstances = new List<ServiceDescriptor>();
                    var serviceDescriptors = services.Where(x => typeof(TService) == x.ServiceType);

                    serviceInstances.AddRange(serviceDescriptors);
                    var instancesOfService = serviceInstances.Where(x =>
                        x.ImplementationType is not null &&
                        typeof(IMustBeInstance).IsAssignableFrom(x.ImplementationType) == false);

                    foreach (var descriptor in instancesOfService)
                        services.Remove(descriptor);

                    services.AddService(ImplementationFactory, serviceLifetime);

                    break;
                case ServiceRegisterStrategy.Replace:
                    services.ReplaceService(ImplementationFactory, serviceLifetime);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(registerStrategy), registerStrategy, null);
            }

            services.TryAddSingleton<Func<IEnumerable<TService>>>(x => x.GetServices<TService>);
            services.TryAddSingleton<ServiceFactory<TService>>();

            return services;
        }

        public static TServiceType? GetSafeService<TServiceType>([NotNull] this IServiceProvider serviceProvider)
            where TServiceType : class
        {
            var serviceType = typeof(TServiceType);

            return serviceProvider.GetSafeService(serviceType) as TServiceType;
        }

        public static object? GetSafeService([NotNull] this IServiceProvider serviceProvider, Type serviceType)
        {
            try
            {
                var services = serviceProvider.GetServices(serviceType);
                return services.FirstOrDefault(serviceType.IsInstanceOfType);
            }
            catch (Exception e)
            {
                try
                {
                    using var scope = serviceProvider.CreateScope();
                    var services = scope.ServiceProvider.GetServices(serviceType);
                    return services.FirstOrDefault(serviceType.IsInstanceOfType);
                }
                catch (Exception exception)
                {
                    throw DomainException.For(Result.Fail($"Fail to get {serviceType.Name}"))
                        .AddExceptionMessage(e).AddExceptionMessage(exception);
                }
            }
        }

        public static TServiceType GetSafeRequiredService<TServiceType>([NotNull] this IServiceProvider serviceProvider)
            where TServiceType : class
        {
            var serviceType = typeof(TServiceType);

            return (TServiceType) serviceProvider.GetSafeRequiredService(serviceType);
        }


        public static object GetSafeRequiredService([NotNull] this IServiceProvider serviceProvider, Type serviceType)
        {
            try
            {
                var services = serviceProvider.GetServices(serviceType);
                return services.First(serviceType.IsInstanceOfType)!;
            }
            catch
            {
                try
                {
                    using var scope = serviceProvider.CreateScope();
                    var services = scope.ServiceProvider.GetServices(serviceType);
                    return services.First(serviceType.IsInstanceOfType)!;
                }
                catch (Exception exception)
                {
                    throw new ServiceCollectionException($"Can't resolve any implementation of {serviceType.Name}",
                        exception);
                }
            }
        }

        public static IConfigurePluginBuilder AddServiceFactories(this IConfigurePluginBuilder builder,
            ServiceLifetime funcLifetime = ServiceLifetime.Transient)
        {
            var services = builder.Services;
            try
            {
                var types = services
                    .Where(x => x.ImplementationType is not null)
                    .Select(x => x.ImplementationType)
                    .SelectMany(x => x?.GetConstructors(BindingFlags.Public | BindingFlags.Instance)!)
                    .SelectMany(x => x.GetParameters())
                    .Select(x => x.ParameterType)
                    .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(Func<>));

                var funcTypes = new HashSet<Type>(types);

                foreach (var funcType in funcTypes)
                {
                    var type = funcType.GetGenericArguments().First();
                    var func = FuncBuilder(type);
                    if (func is null)
                        continue;
                    services.AddService(funcType, func, funcLifetime);
                }

                return builder;
            }
            catch
            {
                return builder;
            }
        }

        public static IServiceCollection RegisterHealthCheck(this IServiceCollection services,
            HealthCheckRegistration registration)
        {
            if (registration == null)
                throw new ArgumentNullException(nameof(registration));

            services.Configure<HealthCheckServiceOptions>(options => { options.Registrations.Add(registration); });
            return services;
        }

        private static Func<IServiceProvider, object>? FuncBuilder(Type type)
        {
            try
            {
                var serviceProvider = Expression.Parameter(typeof(IServiceProvider), "serviceProvider");
                var method = GetServiceMethod.MakeGenericMethod(type);
                var call = Expression.Call(method, serviceProvider);
                var returnType = typeof(Func<>).MakeGenericType(type);
                var returnFunc = Expression.Lambda(returnType, call);
                var func = Expression.Lambda(
                    typeof(Func<,>).MakeGenericType(typeof(IServiceProvider), returnType), returnFunc, serviceProvider);
                var factory = func.Compile() as Func<IServiceProvider, object>;
                return factory;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return default;
            }
        }
    }
}