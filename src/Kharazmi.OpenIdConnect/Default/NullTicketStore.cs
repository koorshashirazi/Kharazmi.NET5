using System.Threading.Tasks;
using Kharazmi.Dependency;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Kharazmi.OpenIdConnect.Default
{
    internal class NullTicketStore : ITicketStore, INullInstance, IShouldBeSingleton, IMustBeInstance
    {
        public Task<string> StoreAsync(AuthenticationTicket ticket)
            => Task.FromResult(string.Empty);

        public Task RenewAsync(string key, AuthenticationTicket ticket)
            => Task.CompletedTask;

        public Task<AuthenticationTicket> RetrieveAsync(string key)
            => Task.FromResult<AuthenticationTicket>(default!);

        public Task RemoveAsync(string key)
            => Task.CompletedTask;
    }
}