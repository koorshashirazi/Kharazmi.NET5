#region

using System;
using System.Collections.Generic;
using System.Linq;
using Kharazmi.BuilderExtensions;
using Kharazmi.ConfigurePlugins;
using Kharazmi.Dependency;
using Kharazmi.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

#endregion

namespace Kharazmi
{
    /// <summary> </summary>
    internal sealed class ConfigurePluginBuilder : IConfigurePluginBuilder, IDisposable
    {
        private readonly Func<IEnumerable<IConfigurePlugin>, IReadOnlyCollection<IConfigurePlugin>>? _filteredPlugin;
        private static readonly List<IConfigurePlugin> Plugins = new();
        private static readonly List<ConfigurePluginBuilderDescriptor> BuilderServiceDescriptors = new();

        /// <summary> </summary>
        public ConfigurePluginBuilder(IServiceCollection services,
            Func<IEnumerable<IConfigurePlugin>, IReadOnlyCollection<IConfigurePlugin>>? filteredPlugin)
        {
            _filteredPlugin = filteredPlugin;
            Services = services;
        }

        public event Action? PostInitializeConfigurePlugin;

        /// <summary> </summary>
        public IServiceCollection Services { get; }

        public IServiceCollection Build()
        {
            var sp = Services.BuildServiceProvider();
            var settings = Services.GetSettings();
            var plugins = sp.GetServices<IConfigurePlugin>().ToList();

            Plugins.AddRange(plugins);

            var filteredPlugins = _filteredPlugin?.Invoke(Plugins) ?? Plugins;

            foreach (var plugin in filteredPlugins)
                plugin.Configure(settings);

            foreach (var plugin in filteredPlugins)
                plugin.Initialize(Services, settings);

            PostInitializeConfigurePlugin?.Invoke();

            settings.SaveChanges();

            Services.RemoveAll<IConfigurePlugin>();

            foreach (var serviceDescriptor in BuilderServiceDescriptors.Where(x => x.ReleaseAfterBuild)
                .Select(x => x.BuilderDescriptor))
                Services.Remove(serviceDescriptor);

            BuilderServiceDescriptors.Clear();

            Services.AddService<IDynamicServiceProvider>(_ => new DynamicServiceProvider(Services),
                ServiceLifetime.Singleton);
            return Services;
        }

        public IConfigurePluginBuilder AddPlugins(IEnumerable<IConfigurePlugin> plugins)
        {
            Plugins.AddRange(plugins);
            return this;
        }

        public IConfigurePluginBuilder AddBuilder<TBuilder>(TBuilder builder, bool releaseAfterBuild = true)
            where TBuilder : class
        {
            BuilderServiceDescriptors.Add(new ConfigurePluginBuilderDescriptor(builder.GetType(),
                new ServiceDescriptor(builder.GetType(), builder), releaseAfterBuild));
            Services.AddSingleton(builder);
            return this;
        }

        public IConfigurePluginBuilder RemoveBuilder<TBuilder>(TBuilder builder) where TBuilder : class
        {
            var descriptor = BuilderServiceDescriptors.FirstOrDefault(x => x.BuilderType == typeof(TBuilder));
            BuilderServiceDescriptors.Remove(descriptor);
            Services.AddSingleton(descriptor.BuilderDescriptor);
            return this;
        }

        public T? GetBuilder<T>() where T : class
            => Services.BuildServiceProvider().GetService<T>();

        public void Dispose()
        {
            Plugins.Clear();
            BuilderServiceDescriptors.Clear();
        }
    }
}