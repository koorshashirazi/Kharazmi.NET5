using System;
using System.Threading.Tasks;
using Kharazmi.Configuration;
using Kharazmi.Constants;
using Kharazmi.Dependency;
using Kharazmi.Http;
using Kharazmi.Options.Cookie;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Logging;

namespace Kharazmi.OpenIdConnect.CookieHandlers
{
    /// <summary>_</summary>
    public interface ICookieUnauthorizedRequestHandler
    {
        /// <summary>_</summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Task HandleAsync(CookieValidatePrincipalContext context);
    }

    internal class CookieUnauthorizedRequestHandler : ICookieUnauthorizedRequestHandler
    {
        private readonly ISettingProvider _settingProvider;
        private readonly IUserContextAccessor _userContextAccessor;
        private readonly ILogger<CookieUnauthorizedRequestHandler>? _logger;

        public CookieUnauthorizedRequestHandler(ISettingProvider settingProvider,
            ServiceFactory<IUserContextAccessor> factory,
            ILogger<CookieUnauthorizedRequestHandler>? logger)
        {
            _settingProvider = settingProvider;
            _userContextAccessor = factory.Instance();
            _logger = logger;
        }

        public async Task HandleAsync(CookieValidatePrincipalContext context)
        {
            try
            {
                context.RejectPrincipal();
                context.Response.StatusCode = 401;
                context.ShouldRenew = true;

                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                _logger?.LogTrace(MessageTemplate.CookieSignOut, MessageEventName.Cookie,
                    nameof(CookieUnauthorizedRequestHandler), CookieAuthenticationDefaults.AuthenticationScheme);

                var openIdConfig = _settingProvider.Get<ExtendedOpenIdOption>();

                if (openIdConfig is null)
                    return;

                var discoveryTokenOptions = DiscoveryTokenOptions.For(
                    openIdConfig.AuthorityUrl,
                    openIdConfig.ClientId,
                    openIdConfig.ClientSecret);


                await Revoke(_userContextAccessor, discoveryTokenOptions);
            }
            catch (Exception e)
            {
                _logger?.LogError("{Message}", e.Message);
            }
        }

        private async Task Revoke(IUserContextAccessor userContextAccessor,
            DiscoveryTokenOptions discoveryTokenOptions)
        {
            try
            {
                await userContextAccessor.RevokeAccessToken(discoveryTokenOptions);
                await userContextAccessor.RevokeRefreshToken(discoveryTokenOptions);
                _logger?.LogTrace("Revoked access token");
                _logger?.LogTrace("Revoked refresh token");
            }
            catch (Exception e)
            {
                _logger?.LogError("{Message}", e.Message);
            }
        }
    }
}