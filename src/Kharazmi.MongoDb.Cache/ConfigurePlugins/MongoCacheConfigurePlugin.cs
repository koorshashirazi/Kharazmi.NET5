using Kharazmi.Configuration;
using Kharazmi.ConfigurePlugins;
using Kharazmi.Extensions;
using Kharazmi.Localization;
using Kharazmi.Localization.Default;
using Kharazmi.Options.Mongo;
using Microsoft.Extensions.DependencyInjection;

namespace Kharazmi.MongoDb.Cache.ConfigurePlugins
{
    internal class MongoCacheConfigurePlugin : IConfigurePlugin
    {
        public string PluginName => nameof(MongoCacheConfigurePlugin);

        public void Configure(ISettingProvider settings)
        {
        }

        public void Initialize(IServiceCollection services, ISettingProvider settings)
        {
            services.RegisterWithFactory<IMongoFactory, MongoCacheFactory, NullMongoFactory,
                MongoOptions>((st, op) => op.Enable && op.UseSecondLevelCache);
        }
    }
}