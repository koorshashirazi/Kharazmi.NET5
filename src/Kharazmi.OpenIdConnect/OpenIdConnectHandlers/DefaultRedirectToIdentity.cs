using System.Threading.Tasks;
using Kharazmi.Extensions;
using Kharazmi.Options.Cookie;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace Kharazmi.OpenIdConnect.OpenIdConnectHandlers
{
    /// <summary>_</summary>
    public interface IRedirectToIdentity
    {
        /// <summary>_</summary>
        /// <param name="context"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        Task HandleAsync(RedirectContext context, ExtendedOpenIdOption option);
    }

    internal class DefaultRedirectToIdentity : IRedirectToIdentity
    {
        public Task HandleAsync(RedirectContext context, ExtendedOpenIdOption option)
        {
            if (context.ProtocolMessage is null) return Task.CompletedTask;

            context.ProtocolMessage.RedirectUri = option.RedirectUri;

            if (option.PostLogoutRedirectUris.IsNotEmpty())
                context.ProtocolMessage.PostLogoutRedirectUri = option.PostLogoutRedirectUris;
            return Task.CompletedTask;
        }
    }
}