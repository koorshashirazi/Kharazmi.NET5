#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Kharazmi.BuilderExtensions;
using Kharazmi.Extensions;
using Kharazmi.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;

#endregion

namespace Kharazmi.Mvc.Globalization
{
    public static class GlobalizationExtensions
    {
        public static RequestLocalizationOptions SetRequestLocalizationOptions(this RequestLocalizationOptions options,
            GlobalizationOption globalizationOption)
        {
            var supportedCultures = globalizationOption.SupportedCultures
                .Select(culture => new CultureInfo(culture)).ToList();

            var defaultCulture = globalizationOption.DefaultSupportedCulture ?? "en";

            options.DefaultRequestCulture = new RequestCulture(defaultCulture, defaultCulture);
            options.SupportedCultures = supportedCultures;
            options.SupportedUICultures = supportedCultures;

            options.RequestCultureProviders = new List<IRequestCultureProvider>
            {
                new QueryStringRequestCultureProvider(),
                new CustomRequestCultureProvider(context =>
                {
                    var cookieName = globalizationOption.CookieName.IsEmpty()
                        ? CookieRequestCultureProvider.DefaultCookieName
                        : globalizationOption.CookieName ?? CookieRequestCultureProvider.DefaultCookieName;

                    var cultureCookieName = context.Request.Cookies[cookieName];

                    string culture;
                    string uiCulture;
                    if (!string.IsNullOrWhiteSpace(cultureCookieName))
                    {
                        var cultures = cultureCookieName.Split('|');
                        var c = cultures[0].Replace("c=", "");
                        var uic = cultures[1].Replace("uic=", "");

                        culture = supportedCultures.FirstOrDefault(x => x.Name.Contains(c))?.Name ?? defaultCulture;
                        uiCulture = supportedCultures.FirstOrDefault(x => x.Name.Contains(uic))?.Name ?? defaultCulture;
                    }
                    else
                    {
                        var userLanguage = context.Request.Headers["Accept-Language"].ToString();
                        var firstLang = userLanguage.Split(',').FirstOrDefault();

                        var userLangCulture =
                            supportedCultures.FirstOrDefault(x => x.Name.Contains(firstLang ?? defaultCulture));

                        culture = userLangCulture != null
                            ? userLangCulture.Name
                            : defaultCulture;

                        uiCulture = userLangCulture != null
                            ? userLangCulture.Name
                            : defaultCulture;
                    }

                    return Task.FromResult(new ProviderCultureResult(culture, uiCulture));
                })
            };

            return options;
        }

        public static IApplicationBuilder UseLocalization(this IApplicationBuilder app,
            Action<RequestLocalizationOptions>? options = null)
        {
            var option = new RequestLocalizationOptions();
            options?.Invoke(option);

            app.UseRequestLocalization(
                option.SetRequestLocalizationOptions(app.ApplicationServices.GetSettings().Get<GlobalizationOption>()));
            return app;
        }
       
    }
}