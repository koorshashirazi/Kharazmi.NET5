using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Kharazmi.Configuration;
using Kharazmi.Options.Cookie;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;

namespace Kharazmi.OpenIdConnect
{
    /// <summary>_</summary>
    internal class OpenIdClaimsTransformation : IClaimsTransformation
    {
        private readonly ILogger<OpenIdClaimsTransformation>? _logger;
        private readonly ISettingProvider _settingProvider;

        /// <summary>_</summary>
        /// <param name="logger"></param>
        /// <param name="settingProvider"></param>
        public OpenIdClaimsTransformation(
            ILogger<OpenIdClaimsTransformation>? logger,
            ISettingProvider settingProvider)
        {
            _logger = logger;
            _settingProvider = settingProvider;
        }

        /// <summary>_</summary>
        /// <param name="principal"></param>
        /// <returns></returns>
        public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            if (!(principal.Identity is ClaimsIdentity identity) || IsNtlm(identity)) return Task.FromResult(principal);

            var openidOptions = _settingProvider.Get<ExtendedOpenIdOption>();

            if (openidOptions.ClaimsForRemove.Length > 0)
                foreach (var claimType in openidOptions.ClaimsForRemove)
                {
                    var claim = identity.Claims.FirstOrDefault(x => x.Type == claimType);
                    if (claim != null)
                        identity.RemoveClaim(claim);
                }


            foreach (var userClaim in identity.Claims)
                _logger?.LogTrace("UserClaims: Type: {Type}, Value: {Value}", userClaim.Type, userClaim.Value);

            return Task.FromResult(principal);
        }

        private static bool IsNtlm(IIdentity identity)
        {
            return identity.AuthenticationType == "Windows" ||
                   identity.AuthenticationType == "Negotiate" ||
                   identity.AuthenticationType == "NYLM";
        }
    }
}