using Kharazmi.BuilderExtensions;
using Kharazmi.Options.Cookie;
using Microsoft.AspNetCore.Builder;

namespace Kharazmi.OpenIdConnect.Extensions
{
    /// <summary>_</summary>
    public static class FrameworkBuilderExtensions
    {
        /// <summary>_</summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IConfigurePluginBuilder AddOpenIdConnectConfigurePlugin(this IConfigurePluginBuilder builder)
        {
            builder.AddConfigurePluginsFrom(new[] {typeof(AssemblyType).Assembly});
            return builder;
        }

        /// <summary>_</summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseOpenIdConnectAuthentication(this IApplicationBuilder builder)
        {
            var settings = builder.GetSettings();
            var option = settings.Get<ExtendedCookieOption>();
            if (option.Enable == false) return builder;
            
            builder.UseAuthentication().UseAuthorization();
            return builder;
        }
    }

}