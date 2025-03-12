using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;

namespace Kharazmi.OpenIdConnect.OpenIdConnectHandlers
{
    /// <summary>_</summary>
    public interface IRemoteFailureHandler
    {
        /// <summary>_</summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Task HandleAsync(RemoteFailureContext context);
    }

    internal class RemoteFailureHandler : IRemoteFailureHandler
    {
        public Task HandleAsync(RemoteFailureContext context)
        {
            context.Response.Redirect(context.Properties?.GetString("returnUrl") ?? "/");
            context.HandleResponse();
            return Task.CompletedTask;
        }
    }
}