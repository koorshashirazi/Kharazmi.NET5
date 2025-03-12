#region

using System;
using Kharazmi.BuilderExtensions;
using Kharazmi.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

#endregion

namespace Kharazmi.Mvc
{
    public static class PluginBuilderExtensions
    {
        public static IConfigurePluginBuilder AddMvcConfigurePlugin(this IConfigurePluginBuilder builder,
            Action<MvcOption>? mvcSetup = null)
        {
            builder.AddConfigurePluginsFrom(new[] {typeof(AssemblyType).Assembly});

            MvcOption option = new();
            mvcSetup?.Invoke(option);

            var mvcBuilder = builder.Services.AddMvc()
                .SetCompatibilityVersion(option.MvcVersion)
                .AddNewtonsoftJson();

            if (option.ControllersAsServices)
                mvcBuilder.AddControllersAsServices();

            if (option.UseSessionStateTempData)
                mvcBuilder.AddSessionStateTempDataProvider();

            if (option.UseLocalization)
                mvcBuilder.AddMvcLocalization(options => options.ResourcesPath = option.ResourcesPath);

            builder.AddBuilder(mvcBuilder);
            return builder;
        }
        
        public static IHttpContextAccessor GetHttpContextAccessor(this IServiceProvider sp)
            => sp.GetInstance<IHttpContextAccessor>();

        public static IUserContextAccessor GetUserHttpContextAccessor(this IServiceProvider sp)
            => sp.GetInstance<IUserContextAccessor>();

        public static IUrlHelper GetUrlHelper(this IServiceProvider sp)
            => sp.GetRequiredService<IUrlHelper>();
    }
}