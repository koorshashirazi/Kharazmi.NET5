using Kharazmi.Configuration;
using Kharazmi.ConfigurePlugins;
using Kharazmi.Extensions;
using Kharazmi.Mvc.Http;
using Kharazmi.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Kharazmi.Mvc.ConfigurePlugins
{
    internal class WorkContextConfigurePlugin : IConfigurePlugin
    {
        public string PluginName => nameof(WorkContextConfigurePlugin);

        public void Configure(ISettingProvider settings)
        {
            WorkContextOption option = settings.Get<WorkContextOption>();

            settings.UpdateOption(option);
        }

        public void Initialize(IServiceCollection services, ISettingProvider settings)
        {
            services.TryAddService<IHttpContextAccessor, HttpContextAccessor>(ServiceLifetime.Singleton);
            services.TryAddService<IActionContextAccessor, ActionContextAccessor>(ServiceLifetime.Singleton);
            services.TryAddSingleton<IUrlHelper>(serviceProvider =>
            {
                var actionContext = serviceProvider.GetService<IActionContextAccessor>()?.ActionContext;
                var urlHelperFactory = serviceProvider.GetService<IUrlHelperFactory>();

                return actionContext is null || urlHelperFactory is null
                    ? new UrlHelper(new ActionContext())
                    : urlHelperFactory.GetUrlHelper(actionContext);
            });

            services.RegisterWithFactory<IHttpRequestAccessor, HttpRequestAccessor, NullHttpRequestAccessor,
                WorkContextOption>((_, op) => op.UseHttpRequestAccessor);
        }
    }
}