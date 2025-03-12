using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace Kharazmi.OpenIdConnect.OpenIdConnectHandlers
{
    /// <summary>_</summary>
    public interface IRedirectToIdentityProviderForSignOut
    {
        /// <summary>_</summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Task HandleAsync(RedirectContext context);
    }

    internal class NullRedirectToIdentityProviderForSignOut : IRedirectToIdentityProviderForSignOut
    {
        public Task HandleAsync(RedirectContext context)
            => Task.CompletedTask;
    }
}