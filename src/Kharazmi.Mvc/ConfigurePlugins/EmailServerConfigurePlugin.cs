using Kharazmi.Configuration;
using Kharazmi.ConfigurePlugins;
using Kharazmi.Extensions;
using Kharazmi.Mvc.MailServer;
using Kharazmi.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Kharazmi.Mvc.ConfigurePlugins
{
    internal class EmailServerConfigurePlugin : IConfigurePlugin
    {
        public string PluginName => nameof(EmailServerConfigurePlugin);
        public void Configure(ISettingProvider settings)
        {
            var option = settings.Get<EmailServerOption>();

            settings.UpdateOption(option);
        }

        public void Initialize(IServiceCollection services, ISettingProvider settings)
        {
            services.RegisterWithFactory<IEmailService, EmailService, NullEmailService, EmailServerOption>(
                (_, op) => op.Enable);
        }
    }
}