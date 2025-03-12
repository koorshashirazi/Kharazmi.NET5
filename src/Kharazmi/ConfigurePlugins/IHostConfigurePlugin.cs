using Kharazmi.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Kharazmi.ConfigurePlugins
{
    public interface IHostConfigurePlugin
    {
        void Configure(HostBuilderContext context, ISettingProvider settings);
        void Initialize(HostBuilderContext context, IServiceCollection services, ISettingProvider settings);
    }
}