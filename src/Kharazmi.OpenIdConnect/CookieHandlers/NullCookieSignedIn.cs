using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;

#pragma warning disable 1591

namespace Kharazmi.OpenIdConnect.CookieHandlers
{
    public interface ICookieSignedIn
    {
        Task HandleAsync(CookieSignedInContext context);
    }
    internal class NullCookieSignedIn : ICookieSignedIn
    {
        public Task HandleAsync(CookieSignedInContext context)
            => Task.CompletedTask;
    }
}