using Hangfire;
using Kharazmi.Background;
using Kharazmi.BuilderExtensions;
using Kharazmi.Configuration;
using Kharazmi.ConfigurePlugins;
using Kharazmi.Extensions;
using Kharazmi.Hangfire.Default;
using Kharazmi.Hangfire.Extensions;
using Kharazmi.Options.Hangfire;
using Microsoft.Extensions.DependencyInjection;

namespace Kharazmi.Hangfire.ConfigurePlugins
{
    internal class HangfireConfigurePlugin : IConfigurePlugin
    {
        public string PluginName => nameof(HangfireConfigurePlugin);

        public void Configure(ISettingProvider settings)
        {
            var option = settings.Get<HangfireOption>();

            settings.UpdateOption(option);
        }

        public void Initialize(IServiceCollection services, ISettingProvider settings)
        {
            services.AddService<global::Hangfire.Logging.ILogProvider, DefaultLogProvider>(ServiceLifetime.Singleton);

            services.AddHangfire(config => services.GetSettings().Get<HangfireOption>().SetConfig(config, services));

            services.RegisterWithFactory<IBackgroundService, HangfireBackgroundService, NullBackgroundService,
                HangfireOption>((_, op) => op.Enable);
            
            services.AddHangfireServer();
        }
    }
}