using Kharazmi.Configuration;
using Kharazmi.Extensions;
using Kharazmi.RealTime;
using Microsoft.Extensions.DependencyInjection;

namespace Kharazmi.ConfigurePlugins
{
    public class SignalRDefaultConfigurePlugin : IConfigurePlugin
    {
        public string PluginName => nameof(SignalRDefaultConfigurePlugin);
        public void Configure(ISettingProvider settings)
        {
            
        }

        public void Initialize(IServiceCollection services, ISettingProvider settings)
        {
           services.RegisterWithFactory<IHubClientPublisher, NullHubClientPublisher>();
           services.RegisterWithFactory<IHubClientSubscriber, NullHubClientSubscriber>();
           services.RegisterWithFactory<IHubClientStore, NullHubClientStore>();
        }
    }
}