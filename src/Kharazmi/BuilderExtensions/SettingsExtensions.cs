using System;
using Kharazmi.Configuration;
using Kharazmi.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Kharazmi.BuilderExtensions
{
    public static class SettingsExtensions
    {
        internal static IHostBuilder AddPluginSettings(this IHostBuilder builder,
            Func<HostBuilderContext, IConfigurationBuilder> configuration, string? jsonFile = "")
        {
            builder.ConfigureServices((ctx, services) =>
            {
                jsonFile = jsonFile.IsNotEmpty()
                    ? jsonFile
                    : $"appsettings.{ctx.HostingEnvironment.EnvironmentName}.json";
                services.ConfigurePluginSettings(configuration(ctx), jsonFile);
            });
            return builder;
        }
    }
}