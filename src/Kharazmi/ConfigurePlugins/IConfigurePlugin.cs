using Kharazmi.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Kharazmi.ConfigurePlugins
{
    public interface IConfigurePlugin
    {
        string PluginName { get; }
        void Configure(ISettingProvider settings);
        void Initialize(IServiceCollection services, ISettingProvider settings);
    }
}