using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;

namespace Kharazmi.OpenIdConnect.Default
{
    internal sealed class DisabledAuthorization : IPolicyEvaluator
    {
        public Task<AuthenticateResult> AuthenticateAsync(AuthorizationPolicy policy, HttpContext context)
        {
            var authenticationTicket = new AuthenticationTicket(new ClaimsPrincipal(),
                new AuthenticationProperties(), CookieAuthenticationDefaults.AuthenticationScheme);
            return Task.FromResult(AuthenticateResult.Success(authenticationTicket));
        }

        public Task<PolicyAuthorizationResult> AuthorizeAsync(AuthorizationPolicy policy,
            AuthenticateResult authenticationResult, HttpContext context, object? resource)
        {
            return Task.FromResult(PolicyAuthorizationResult.Success());
        }
    }
}