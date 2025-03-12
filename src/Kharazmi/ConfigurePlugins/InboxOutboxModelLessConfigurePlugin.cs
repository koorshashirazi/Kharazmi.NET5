using Kharazmi.Configuration;
using Kharazmi.Extensions;
using Kharazmi.Outbox;
using Microsoft.Extensions.DependencyInjection;

namespace Kharazmi.ConfigurePlugins
{
    internal class InboxOutboxModelLessConfigurePlugin : IConfigurePlugin
    {
        public string PluginName => nameof(InboxOutboxModelLessConfigurePlugin);

        public void Configure(ISettingProvider settings)
        {
        }

        public void Initialize(IServiceCollection services, ISettingProvider settings)
        {
            services
                .RegisterWithFactory<IInboxOutboxProvider, NullInboxOutboxProvider>();
        }
    }
}