using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Kharazmi.OpenIdConnect.CookieHandlers
{
    public interface ICookieSigningOut
    {
        Task HandleAsync(CookieSigningOutContext context);
    }

    internal class NullCookieSigningOut : ICookieSigningOut
    {
        public Task HandleAsync(CookieSigningOutContext context)
            => Task.CompletedTask;
    }
}