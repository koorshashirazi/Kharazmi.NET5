#region

using System;
using System.Threading.Tasks;
using Kharazmi.Guard;
using Kharazmi.OpenIdConnect.CookieHandlers;
using Kharazmi.Options.Cookie;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

#endregion

namespace Kharazmi.OpenIdConnect.Extensions
{
    /// <summary>_</summary>
    internal static class CookieExtensions
    {
        /// <summary>_</summary>
        /// <param name="options"></param>
        /// <param name="cookieOptions"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        internal static CookieAuthenticationOptions ConfigureExtendedCookieAuthentication(
            this ExtendedCookieAuthenticationOptions options, ExtendedCookieOption? cookieOptions)
        {
            cookieOptions = cookieOptions.NotNull(nameof(cookieOptions));

            options.Cookie.SameSite = SameSiteMode.Unspecified;
            options.LoginPath = new PathString(cookieOptions.LoginPath);
            options.LogoutPath = new PathString(cookieOptions.LogoutPath);
            options.AccessDeniedPath = new PathString(cookieOptions.AccessDeniedPath);
            options.ReturnUrlParameter =
                cookieOptions.ReturnUrlParameter ?? CookieAuthenticationDefaults.ReturnUrlParameter;

            options.Events.OnValidatePrincipal = OnValidatePrincipal;
            options.Events.OnSignedIn = OnSignedIn;
            options.Events.OnSigningIn = OnSigningIn;
            options.Events.OnSigningOut = OnSigningOut;
            options.Events.OnRedirectToLogin = OnRedirectToLogin;
            options.Events.OnRedirectToLogout = OnRedirectToLogout;
            options.Events.OnRedirectToAccessDenied = OnRedirectToAccessDenied;
            options.Events.OnRedirectToReturnUrl = OnRedirectToReturnUrl;

            return options;
        }

        private static Task OnValidatePrincipal(CookieValidatePrincipalContext context)
        {
            var handlers = context.HttpContext.RequestServices.GetService<ICookieValidationHandler>();
            return handlers?.HandleAsync(context) ?? Task.CompletedTask;
        }

        private static Task OnRedirectToReturnUrl(RedirectContext<CookieAuthenticationOptions> context)
        {
            var handlers = context.HttpContext.RequestServices.GetService<ICookieRedirectToReturnUrl>();
            return handlers?.HandleAsync(context) ?? Task.CompletedTask;
        }

        private static Task OnRedirectToAccessDenied(RedirectContext<CookieAuthenticationOptions> context)
        {
            var handlers = context.HttpContext.RequestServices.GetService<ICookieRedirectToAccessDenied>();
            return handlers?.HandleAsync(context) ?? Task.CompletedTask;
        }

        private static Task OnRedirectToLogout(RedirectContext<CookieAuthenticationOptions> context)
        {
            var handlers = context.HttpContext.RequestServices.GetService<ICookieRedirectToAccessDenied>();
            return handlers?.HandleAsync(context) ?? Task.CompletedTask;
        }

        private static Task OnRedirectToLogin(RedirectContext<CookieAuthenticationOptions> context)
        {
            var handlers = context.HttpContext.RequestServices.GetService<ICookieRedirectToLogout>();
            return handlers?.HandleAsync(context) ?? Task.CompletedTask;
        }

        private static Task OnSigningOut(CookieSigningOutContext context)
        {
            var handlers = context.HttpContext.RequestServices.GetService<ICookieSigningOut>();
            return handlers?.HandleAsync(context) ?? Task.CompletedTask;
        }

        private static Task OnSigningIn(CookieSigningInContext context)
        {
            var handlers = context.HttpContext.RequestServices.GetService<ICookieSigningIn>();
            return handlers?.HandleAsync(context) ?? Task.CompletedTask;
        }

        private static Task OnSignedIn(CookieSignedInContext context)
        {
            var handlers = context.HttpContext.RequestServices.GetService<ICookieSignedIn>();
            return handlers?.HandleAsync(context) ?? Task.CompletedTask;
        }
    }
}