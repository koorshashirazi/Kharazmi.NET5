using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace Kharazmi.OpenIdConnect.OpenIdConnectHandlers
{
    /// <summary>_</summary>
    public interface IUserInformationReceivedHandler
    {
        /// <summary>_</summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Task HandleAsync(UserInformationReceivedContext context);
    }

    internal class NullUserInformationReceivedHandler : IUserInformationReceivedHandler
    {
        public Task HandleAsync(UserInformationReceivedContext context)
            => Task.CompletedTask;
    }
}