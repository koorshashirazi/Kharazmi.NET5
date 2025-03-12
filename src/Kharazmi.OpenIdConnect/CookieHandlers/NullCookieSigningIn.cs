using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Kharazmi.OpenIdConnect.CookieHandlers
{
    public interface ICookieSigningIn
    {
        Task HandleAsync(CookieSigningInContext context);
    }

    internal class NullCookieSigningIn : ICookieSigningIn
    {
        public Task HandleAsync(CookieSigningInContext context)
            => Task.CompletedTask;
    }
}