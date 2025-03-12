using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kharazmi.BuilderExtensions;
using Kharazmi.Configuration;
using Kharazmi.ConfigurePlugins;
using Kharazmi.Constants;
using Kharazmi.Extensions;
using Kharazmi.Localization.Default;
using Kharazmi.Options.Mongo;
using Microsoft.Extensions.DependencyInjection;

namespace Kharazmi.Localization.ConfigurePlugins
{
    internal class MongoConfigurePlugin : IConfigurePlugin
    {
        private static readonly HashSet<Assembly> AssembliesCached = new();
        public string PluginName => nameof(MongoConfigurePlugin);

        public void Configure(ISettingProvider settings)
        {
            var option = settings.Get<MongoOptions>();

            settings.UpdateOption(option);
        }

        public void Initialize(IServiceCollection services, ISettingProvider settings)
        {
            var options = settings.Get<MongoOptions>();

            if (options.UseDatabaseInstaller)
            {
                foreach (var option in options.ChildOptions)
                {
                    var assemblyInstaller = option.AssemblyDbInstaller;
                    if (assemblyInstaller.IsNotEmpty())
                    {
                        var assembly = Assembly.Load(assemblyInstaller);
                        if (!assembly.IsNull())
                        {
                            if (AssembliesCached.Any(x => x == assembly)) continue;
                            AssembliesCached.Add(assembly);
                        }
                        else
                        {
                            if (option.ThrowIfMigrationFailed)
                                throw new TypeAccessException(MessageHelper.CanNotLoadAssemblyFromType(
                                    MessageEventName.MongoDbInstaller, "DatabaseInstaller", assemblyInstaller));
                        }
                    }
                    else
                    {
                        if (option.ThrowIfMigrationFailed)
                            throw new TypeAccessException(MessageHelper.NullOrEmpty(
                                MessageEventName.MongoDbInstaller, nameof(MongoOption),
                                nameof(MongoOption.AssemblyDbInstaller)));
                    }
                }

                services.ScanAssemblyFor<IMongoDbInstaller>(AssembliesCached.ToArray(),
                    lifetime: ServiceLifetime.Singleton);
                AssembliesCached.Clear();
            }

            services.AddSingleton<MongoDbInstallerMiddleware>();

            services.RegisterWithFactory<IMongoClientPool, MongoClientPool, NullMongoClientPool,
                MongoOptions>((_, op) => op.Enable);

            services.RegisterWithFactory<IMongoFactory, MongoFactory, NullMongoFactory,
                MongoOptions>((_, op) => op.Enable && !op.UseSecondLevelCache);

            services.RegisterWithFactory<IMongoDbContext, MongoDbContext, NullMongoDbContext,
                MongoOptions>((_, op) => op.Enable);
        }
    }
}