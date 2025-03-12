#region

using System;
using System.Collections.Generic;
using System.Reflection;
using Kharazmi.Configuration;
using Kharazmi.ConfigurePlugins;
using Kharazmi.Constants;
using Kharazmi.Dependency;
using Kharazmi.HealthChecks;
using Kharazmi.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Scrutor;

#endregion

namespace Kharazmi.BuilderExtensions
{
    /// <summary> </summary>
    public static class CoreExtensions
    {
        /// <summary> </summary>
        public static IHostConfigurePluginBuilder ConfigurePluginBuilder(this IHostBuilder hostBuilder,
            Func<HostBuilderContext, IConfigurationBuilder> configuration, string? jsonFile = "")
        {
            return new HostConfigurePluginBuilder(hostBuilder, configuration, jsonFile);
        }

        /// <summary> </summary>
        public static IConfigurePluginBuilder AddCoreConfigurePlugin(this IServiceCollection services,
            Func<IEnumerable<IConfigurePlugin>, IReadOnlyCollection<IConfigurePlugin>>? filteredPlugins = null)
        {
            var builder = new ConfigurePluginBuilder(services, filteredPlugins)
                .AddConfigurePluginsFrom(new[] {typeof(AssemblyType).Assembly});

            services.AddSingleton(builder);
            return builder;
        }

        public static IApplicationBuilder UseConfigurePluginChecker(this IApplicationBuilder builder)
        {
            var settings = builder.GetSettings();
            settings.ReloadSettings();

            var sp = builder.ApplicationServices;
            var hooks = sp.GetServices<IHealthCheckerPlugin>();
            var checker = builder.GetFrameworkChecker();

            foreach (var hook in hooks)
                hook.HealthyCheck(checker, settings);

            settings.SaveChanges();
            return builder;
        }

        public static IConfigurePluginBuilder AddConfigurePluginsFrom(this IConfigurePluginBuilder builder,
            Assembly[] assemblies)
        {
            builder.Services.ScanAssemblyFor<IConfigurePlugin>(lifetime: ServiceLifetime.Singleton,
                assemblies: assemblies);
            builder.Services.ScanAssemblyFor<IHealthCheckConfigurePlugin>(lifetime: ServiceLifetime.Singleton,
                assemblies: assemblies);
            builder.Services.ScanAssemblyFor<IHealthCheckerPlugin>(lifetime: ServiceLifetime.Singleton,
                assemblies: assemblies);
            builder.Services.ScanAssembly(assemblies);
            return builder;
        }

        public static IServiceCollection ScanAssemblyFor<TService>(this IServiceCollection services,
            Assembly[] assemblies,
            RegistrationStrategy? strategy = null,
            ServiceLifetime lifetime = ServiceLifetime.Transient) where TService : class
        {
            if (assemblies.Length <= 0)
                throw new TypeAccessException(MessageHelper.CanNotLoadAssemblyFromType(MessageEventName.Assembly,
                    "ScanAssembly", typeof(TService).Name));

            strategy ??= RegistrationStrategy.Append;

            services.Scan(types => types.FromAssemblies(assemblies)
                .AddClasses(c =>
                {
                    c.AssignableTo(typeof(TService));
                    c.Where(t => !t.IsAbstract && t.IsClass);
                })
                .UsingRegistrationStrategy(strategy)
                .AsImplementedInterfaces()
                .WithLifetime(lifetime));

            return services;
        }

        public static IServiceCollection ScanAssemblyFor(this IServiceCollection services, Type typeService,
            Assembly[] assemblies,
            RegistrationStrategy? strategy = null,
            ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            if (assemblies.Length <= 0)
                throw new TypeAccessException(MessageHelper.CanNotLoadAssemblyFromType(MessageEventName.Assembly,
                    "ScanAssembly", typeService.Name));

            strategy ??= RegistrationStrategy.Append;

            services.Scan(types => types.FromAssemblies(assemblies)
                .AddClasses(c =>
                {
                    c.AssignableTo(typeService);
                    c.Where(t => !t.IsAbstract && t.IsClass);
                })
                .UsingRegistrationStrategy(strategy)
                .AsImplementedInterfaces()
                .WithLifetime(lifetime));

            return services;
        }


        public static IServiceCollection ScanAssembly(this IServiceCollection services,
            Assembly[] assemblies, RegistrationStrategy? strategy = null)
        {
            if (assemblies.Length <= 0)
                throw new TypeAccessException(MessageHelper.CanNotLoadAssemblyFromType(MessageEventName.Assembly,
                    "ScanAssembly", ""));

            strategy ??= RegistrationStrategy.Append;

            services.Scan(types => types.FromAssemblies(assemblies)
                .AddClasses(c =>
                {
                    c.WithAttribute<SingletonAttribute>();
                    c.Where(t => !t.IsAbstract && t.IsClass);
                })
                .UsingRegistrationStrategy(strategy)
                .AsImplementedInterfaces()
                .WithLifetime(ServiceLifetime.Singleton));

            services.Scan(types => types.FromAssemblies(assemblies)
                .AddClasses(c =>
                {
                    c.WithAttribute<ScopedAttribute>();
                    c.Where(t => !t.IsAbstract && t.IsClass);
                })
                .UsingRegistrationStrategy(strategy)
                .AsImplementedInterfaces()
                .WithLifetime(ServiceLifetime.Scoped));

            services.Scan(types => types.FromAssemblies(assemblies)
                .AddClasses(c =>
                {
                    c.WithAttribute<TransientAttribute>();
                    c.Where(t => !t.IsAbstract && t.IsClass);
                })
                .UsingRegistrationStrategy(strategy)
                .AsImplementedInterfaces()
                .WithLifetime(ServiceLifetime.Transient));

            return services;
        }

        public static IDynamicServiceProvider GetDynamicServiceProvider(this IServiceCollection services)
            => services.BuildServiceProvider().GetRequiredService<IDynamicServiceProvider>();

        public static IDynamicServiceProvider GetDynamicServiceProvider(this IServiceProvider services)
            => services.GetRequiredService<IDynamicServiceProvider>();

        public static IDynamicServiceProvider GetDynamicServiceProvider(this IApplicationBuilder services)
            => services.ApplicationServices.GetRequiredService<IDynamicServiceProvider>();

        public static IDynamicResolver GetDynamicResolver(this IServiceProvider services)
            => services.GetRequiredService<IDynamicResolver>();


        public static T GetInstance<T>(this IServiceProvider services) where T : class =>
            services.GetRequiredService<ServiceFactory<T>>().Instance();

        public static ILoggerFactory? GetLoggerFactory(this IServiceProvider sp)
            => sp.GetService<ILoggerFactory>();

        public static TSettings? GetAppSettings<TSettings>(this IServiceCollection services)
            where TSettings : class, ISettings
            => GetSettings(services).As<TSettings>();

        public static ISettingProvider GetSettings(this IServiceCollection services)
            => services.BuildServiceProvider().GetRequiredService<ISettingProvider>();

        public static ISettingProvider GetSettings(this IApplicationBuilder app)
            => app.ApplicationServices.GetRequiredService<ISettingProvider>();

        public static ISettingProvider GetSettings(this IServiceProvider services)
            => services.GetRequiredService<ISettingProvider>();

        public static IHttpClientFactory GetHttpClientFactory(this IServiceProvider sp)
            => sp.GetRequiredService<IHttpClientFactory>();

        public static IHealthChecker GetFrameworkChecker(this IApplicationBuilder app)
            => app.ApplicationServices.GetRequiredService<IHealthChecker>();

        public static IOptions<T>? GetOption<T>(this IServiceCollection services) where T : class
            => services.BuildServiceProvider().GetService<IOptions<T>>();
    }
}