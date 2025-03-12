using System;
using System.Collections.Generic;
using System.Linq;
using Kharazmi.BuilderExtensions;
using Kharazmi.ConfigurePlugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Kharazmi.Builders
{
    public class HealthCheckConfigurePluginBuilder: IDisposable
    {
        private static readonly List<IHealthCheckConfigurePlugin> Plugins = new();

        public IConfigurePluginBuilder Builder { get; }
        public IServiceCollection Services { get; }

        public HealthCheckConfigurePluginBuilder(IConfigurePluginBuilder builder)
        {
            Builder = builder;
            Services = builder.Services;
            builder.PostInitializeConfigurePlugin += OnInitializeConfigurePlugin;
        }

        private void OnInitializeConfigurePlugin()
        {
            var sp = Services.BuildServiceProvider();
            var plugins = sp.GetServices<IHealthCheckConfigurePlugin>().ToList();
            AddPlugins(plugins);

            foreach (var plugin in Plugins)
                plugin.Initialize(this, sp.GetSettings());

            Services.RemoveAll<IHealthCheckConfigurePlugin>();
        }

        public HealthCheckConfigurePluginBuilder AddPlugins(IEnumerable<IHealthCheckConfigurePlugin> plugins)
        {
            Plugins.AddRange(plugins);
            return this;
        }

        public HealthCheckConfigurePluginBuilder Add(HealthCheckRegistration registration)
        {
            if (registration == null)
                throw new ArgumentNullException(nameof(registration));

            Services.Configure<HealthCheckServiceOptions>(options => { options.Registrations.Add(registration); });

            return this;
        }


        public void Dispose()
        {
            Builder.PostInitializeConfigurePlugin -= OnInitializeConfigurePlugin;
            Plugins.Clear();
        }

    }
}