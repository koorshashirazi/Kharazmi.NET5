using System.Linq;
using Kharazmi.Constants;
using Kharazmi.Extensions;
using Kharazmi.Guard;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Kharazmi.Configuration
{
    public static class JsonSettingsExtensions
    {
        public static IServiceCollection ConfigurePluginSettings(this IServiceCollection services,
            IConfigurationBuilder builder, string pathJsonFile)
        {
            var configs = builder
                .AddSettingProvider(FrameworkConstants.JsonFileName)
                .AddSettingProvider(pathJsonFile).Build();

            services.AddSingleton(configs);
            services.AddOptions();
            services.AddSingleton<IJsonBackupSettings, JsonBackupSettings>();

            var serviceProvider = services.BuildServiceProvider();
            
            new JsonSettingProvider(serviceProvider,
                serviceProvider.GetRequiredService<IConfigurationRoot>(),
                serviceProvider.GetService<ILogger<JsonSettingProvider>>(),
                serviceProvider.GetRequiredService<IJsonBackupSettings>(),
                serviceProvider.GetRequiredService<IHostEnvironment>()).Init(pathJsonFile);

            services.AddService<ISettingProvider>(sp =>
            {
                var c = ServiceProviderServiceExtensions.GetRequiredService<IConfigurationRoot>(sp);
                var env = ServiceProviderServiceExtensions.GetRequiredService<IHostEnvironment>(sp);
                var bk = ServiceProviderServiceExtensions.GetRequiredService<IJsonBackupSettings>(sp);
                var log = ServiceProviderServiceExtensions.GetService<ILogger<JsonSettingProvider>>(sp);
                return new JsonSettingProvider(sp, c, log, bk, env);
            }, ServiceLifetime.Singleton);

            return services;
        }

        public static IConfigurationBuilder AddSettingProvider(this IConfigurationBuilder builder, string file)
        {
            builder.NotNull(nameof(builder));

            var source = builder.Sources.FirstOrDefault(x => (x as JsonSettingsSource)?.Path == file);
            if (source == null)
                builder.Add(new JsonSettingsSource {Path = file});

            return builder;
        }
    }
}