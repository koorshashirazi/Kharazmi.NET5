using System;
using System.Collections.Generic;
using Kharazmi.BuilderExtensions;
using Kharazmi.ConfigurePlugins;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Kharazmi
{
    public interface IHostConfigurePluginBuilder
    {
        /// <summary> </summary>
        IHostBuilder HostBuilder { get; }

        IHostBuilder Build();

        IHostConfigurePluginBuilder AddPlugins(IEnumerable<IHostConfigurePlugin> plugins);
    }

    internal sealed class HostConfigurePluginBuilder : IHostConfigurePluginBuilder, IDisposable
    {
        private readonly Func<HostBuilderContext, IConfigurationBuilder> _configuration;
        private readonly string? _jsonFile;
        private static readonly List<IHostConfigurePlugin> Plugins = new();


        public HostConfigurePluginBuilder(IHostBuilder hostBuilder,
            Func<HostBuilderContext, IConfigurationBuilder> configuration, string? jsonFile)
        {
            _configuration = configuration;
            _jsonFile = jsonFile;
            HostBuilder = hostBuilder;
        }

        public IHostBuilder HostBuilder { get; }

        public IHostBuilder Build()
        {
            HostBuilder.AddPluginSettings(_configuration, _jsonFile);

            HostBuilder.ConfigureServices((context, services) =>
            {
                var settings = services.GetSettings();
                foreach (var plugin in Plugins)
                    plugin.Configure(context, settings);

                foreach (var plugin in Plugins)
                    plugin.Initialize(context, services, settings);

                settings.SaveChanges();
            });


            return HostBuilder;
        }

        public IHostConfigurePluginBuilder AddPlugins(IEnumerable<IHostConfigurePlugin> plugins)
        {
            Plugins.AddRange(plugins);
            return this;
        }

        public void Dispose()
        {
            Plugins.Clear();
        }
    }
}