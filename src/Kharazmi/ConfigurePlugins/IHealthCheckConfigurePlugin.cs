using Kharazmi.Builders;
using Kharazmi.Configuration;

namespace Kharazmi.ConfigurePlugins
{
    public interface IHealthCheckConfigurePlugin 
    {
        string HealthCheckName { get; }
        void Initialize(HealthCheckConfigurePluginBuilder configurePluginBuilder, ISettingProvider settings);
    }
}