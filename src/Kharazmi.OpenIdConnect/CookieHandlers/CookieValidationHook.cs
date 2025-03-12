using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Kharazmi.Configuration;
using Kharazmi.Extensions;
using Kharazmi.Options.Cookie;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Logging;

namespace Kharazmi.OpenIdConnect.CookieHandlers
{
    /// <summary>Invoke CookieValidatePrincipalContext</summary>
    public interface ICookieValidationHandler
    {
        /// <summary>_</summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Task HandleAsync(CookieValidatePrincipalContext context);
    }

    internal class NullCookieValidationHandler : ICookieValidationHandler
    {
        public Task HandleAsync(CookieValidatePrincipalContext context)
            => Task.CompletedTask;
    }


    internal class CookieValidationHandler : ICookieValidationHandler
    {
        private readonly ISettingProvider _settingProvider;
        private readonly ILogger<CookieValidationHandler>? _logger;
        private readonly ICookieUnauthorizedRequestHandler _unauthorizedRequestHandler;

        public CookieValidationHandler(
            ISettingProvider settingProvider,
            ICookieUnauthorizedRequestHandler unauthorizedRequestHandler,
            ILogger<CookieValidationHandler>? logger)
        {
            _settingProvider = settingProvider;
            _logger = logger;
            _unauthorizedRequestHandler = unauthorizedRequestHandler;
        }

        public async Task HandleAsync(CookieValidatePrincipalContext context)
        {
            var userPrincipal = context.Principal;
            var identity = userPrincipal?.Identity;

            if (!(identity is ClaimsIdentity claimsIdentity) || !claimsIdentity.Claims.Any())
            {
                await _unauthorizedRequestHandler.HandleAsync(context);
                return;
            }

            var cookieOptions = _settingProvider.Get<ExtendedCookieOption>();

            var cookieValidation = cookieOptions.CookieValidateOption;
            if (cookieValidation is null) return;

            var claims = cookieValidation.ClaimsNotAllowEmpty;
            if (claims.IsNull() == false)
            {
                foreach (var claim in claims)
                {
                    var claimValue = claimsIdentity.FindFirstValue(claim);
                    if (!claimValue.IsEmpty())
                    {
                        _logger?.LogTrace("Claim {Claim} is valid: ", claim);
                        continue;
                    }

                    _logger?.LogError("Claim {Claim} is not found: ", claim);
                    await _unauthorizedRequestHandler.HandleAsync(context);
                    return;
                }
            }

            var claimsMustBe = cookieValidation.ClaimsMustBe;
            if (claimsMustBe.IsNull() == false)
            {
                foreach (var claim in claimsMustBe)
                {
                    var claimValue = claimsIdentity.FindFirstValue(claim.Key);

                    if (claimValue.IsNotEmpty() && claimValue.Equals(claim.Value))
                    {
                        _logger?.LogTrace("Claim {Claim} is valid: ", claim);
                        continue;
                    }

                    _logger?.LogError("Claim {Claim} is bout found or is not valid: ", claim);
                    await _unauthorizedRequestHandler.HandleAsync(context);
                    return;
                }
            }
        }
    }
}