using System;
using Kharazmi.Configuration;
using Kharazmi.Constants;
using Kharazmi.Dependency;
using Kharazmi.Extensions;
using Kharazmi.OpenIdConnect.Default;
using Kharazmi.Options.Cookie;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;

namespace Kharazmi.OpenIdConnect
{
    /// <summary>_</summary>
    internal class CustomPostConfigureCookie : IPostConfigureOptions<ExtendedCookieAuthenticationOptions>
    {
        private readonly ISettingProvider _settingProvider;
        private readonly IDataProtectionProvider _dp;
        private readonly ITicketStore? _ticketStore;

        public CustomPostConfigureCookie(
            ISettingProvider settingProvider,
            IDataProtectionProvider dataProtection,
            ServiceFactory<ITicketStore> ticketStore)
        {
            _settingProvider = settingProvider;
            _dp = dataProtection;
            _ticketStore = ticketStore.Instance();
        }

        public void PostConfigure(string name, ExtendedCookieAuthenticationOptions options)
        {
            options.DataProtectionProvider ??= _dp;

            var extendedCookie = _settingProvider.Get<ExtendedCookieOption>();
            var cookieOptions = extendedCookie.CookieOption;
            var expiration = cookieOptions.ExpirationOptions;

            options.Cookie.Name = cookieOptions.CookieName.IsEmpty()
                ? CookieConstants.CookiePrefix + Uri.EscapeDataString(name)
                : cookieOptions.CookieName;

            options.Cookie.IsEssential = cookieOptions.IsEssential;

            options.ExpireTimeSpan = expiration.AbsoluteExpiration ?? ExpirationConstants.AbsoluteExpiration;
            options.SlidingExpiration = expiration.SlidingExpiration > TimeSpan.Zero;

            if (cookieOptions.UseTicketStore && _ticketStore is not NullTicketStore)
            {
                options.SessionStore = _ticketStore;
            }

            if (options.TicketDataFormat.IsNull())
            {
                // Note: the purpose for the data protector must remain fixed for interop to work.
                var dataProtector = options.DataProtectionProvider.CreateProtector(
                    "Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationMiddleware", name, "v2");
                options.TicketDataFormat = new TicketDataFormat(dataProtector);
            }

            options.CookieManager ??= new ChunkingCookieManager();

            if (!options.LoginPath.HasValue)
            {
                options.LoginPath = CookieAuthenticationDefaults.LoginPath;
            }

            if (!options.LogoutPath.HasValue)
            {
                options.LogoutPath = CookieAuthenticationDefaults.LogoutPath;
            }

            if (!options.AccessDeniedPath.HasValue)
            {
                options.AccessDeniedPath = CookieAuthenticationDefaults.AccessDeniedPath;
            }
        }
    }
}