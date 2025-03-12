using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Kharazmi.OpenIdConnect.CookieHandlers
{
    public interface ICookieRedirectToAccessDenied
    {
        Task HandleAsync(RedirectContext<CookieAuthenticationOptions> context);
    }

    internal class NullCookieRedirectToAccessDenied : ICookieRedirectToAccessDenied
    {
        public Task HandleAsync(RedirectContext<CookieAuthenticationOptions> context)
            => Task.CompletedTask;
    }
}