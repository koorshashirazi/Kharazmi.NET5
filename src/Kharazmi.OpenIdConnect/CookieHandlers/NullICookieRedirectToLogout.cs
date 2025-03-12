using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Kharazmi.OpenIdConnect.CookieHandlers
{
    public interface ICookieRedirectToLogout
    {
        Task HandleAsync(RedirectContext<CookieAuthenticationOptions> context);
    }

    internal class NullICookieRedirectToLogout : ICookieRedirectToLogout
    {
        public Task HandleAsync(RedirectContext<CookieAuthenticationOptions> context)
            => Task.CompletedTask;
    }
}