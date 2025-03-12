using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace Kharazmi.OpenIdConnect.OpenIdConnectHandlers
{
    /// <summary>_</summary>
    public interface ITokenValidated
    {
        /// <summary>_</summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Task HandleAsync(TokenValidatedContext context);
    }

    internal class NullTokenValidated : ITokenValidated
    {
        public Task HandleAsync(TokenValidatedContext context)
            => Task.CompletedTask;
    }
}