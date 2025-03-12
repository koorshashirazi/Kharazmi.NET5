#region

using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Kharazmi.Configuration;
using Kharazmi.Constants;
using Kharazmi.Dependency;
using Kharazmi.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Distributed;

#endregion

namespace Kharazmi.OpenIdConnect.SessionStores
{
    internal class DistributedSessionStore : ITicketStore, IShouldBeSingleton, IMustBeInstance
    {
        private readonly ISettingProvider _settingProvider;
        private const string KeyPrefix = "AuthTicket:";
        private readonly IDistributedCache _cache;
        private readonly IDataSerializer<AuthenticationTicket> _ticketSerializer = TicketSerializer.Default;

        public DistributedSessionStore(ISettingProvider settingProvider, ServiceFactory<IDistributedCache> cache)
        {
            _settingProvider = settingProvider;
            _cache = cache.Instance();
        }

        public async Task<string> StoreAsync(AuthenticationTicket ticket)
        {
            var key = $"{KeyPrefix}{Guid.NewGuid():N}";
            await RenewAsync(key, ticket);
            return key;
        }

        public Task RenewAsync(string key, AuthenticationTicket ticket)
        {
            var options = new DistributedCacheEntryOptions();

            var expiresUtc = ticket.Properties.ExpiresUtc;

            switch (ticket.Properties.IsPersistent)
            {
                case false when expiresUtc.HasValue:
                    options.SetAbsoluteExpiration(expiresUtc.Value);
                    break;
                case true when expiresUtc.HasValue:
                    options.SetAbsoluteExpiration(expiresUtc.Value);
                    break;
                case true when !expiresUtc.HasValue:
                    SetDefaultExpirations(options);
                    break;
                default:
                    SetDefaultExpirations(options);
                    break;
            }

            return _cache.SetAsync(key, _ticketSerializer.Serialize(ticket), options);
        }

        public async Task<AuthenticationTicket> RetrieveAsync(string key)
        {
            var value = await _cache.GetAsync(key);
            return value != null
                ? _ticketSerializer.Deserialize(value) ?? new AuthenticationTicket(new ClaimsPrincipal(), "")
                : new AuthenticationTicket(new ClaimsPrincipal(), "");
        }

        public Task RemoveAsync(string key)
            => _cache.RemoveAsync(key);

        private void SetDefaultExpirations(DistributedCacheEntryOptions options)
        {
            var cacheOption = _settingProvider.Get<CacheOption>();
            var expirations = cacheOption.ExpirationOption;
            options.SetAbsoluteExpiration(expirations.AbsoluteExpiration ?? ExpirationConstants.AbsoluteExpiration);
            options.SetSlidingExpiration(expirations.SlidingExpiration ?? ExpirationConstants.SlidingExpiration);
        }
    }
}