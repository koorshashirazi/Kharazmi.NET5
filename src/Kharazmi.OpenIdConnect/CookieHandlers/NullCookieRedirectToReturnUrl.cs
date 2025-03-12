using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

#pragma warning disable 1591

namespace Kharazmi.OpenIdConnect.CookieHandlers
{
    public interface ICookieRedirectToReturnUrl
    {
        Task HandleAsync(RedirectContext<CookieAuthenticationOptions> context);
    }

    internal class NullCookieRedirectToReturnUrl : ICookieRedirectToReturnUrl
    {
        public Task HandleAsync(RedirectContext<CookieAuthenticationOptions> context)
            => Task.CompletedTask;
    }
}