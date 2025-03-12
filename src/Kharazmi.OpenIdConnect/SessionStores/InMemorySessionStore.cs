#region

using System;
using System.Threading.Tasks;
using Kharazmi.Dependency;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Memory;

#endregion

namespace Kharazmi.OpenIdConnect.SessionStores
{
    /// <summary>
    /// Store session in memory
    /// </summary>
    internal class InMemorySessionStore : ITicketStore, IShouldBeSingleton, IMustBeInstance
    {
        private const string KeyPrefix = "AuthTicket-";
        private readonly IMemoryCache _memoryCache;

        /// <summary></summary>
        /// <param name="memoryCache"></param>
        public InMemorySessionStore(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        /// <summary></summary>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task RemoveAsync(string key)
        {
            _memoryCache.Remove(key);
            return Task.CompletedTask;
        }

        /// <summary></summary>
        /// <param name="key"></param>
        /// <param name="ticket"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task RenewAsync(string key, AuthenticationTicket ticket)
        {
            var options = new MemoryCacheEntryOptions {Priority = CacheItemPriority.High};

            var expiresUtc = ticket.Properties.ExpiresUtc;

            if (expiresUtc.HasValue) options.SetAbsoluteExpiration(expiresUtc.Value);
            if (ticket.Properties.AllowRefresh.GetValueOrDefault(false))
                options.SetSlidingExpiration(TimeSpan.FromMinutes(15));

            _memoryCache.Set(key, ticket, options);

            return Task.FromResult(0);
        }

        /// <summary></summary>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task<AuthenticationTicket> RetrieveAsync(string key)
        {
            _memoryCache.TryGetValue(key, out AuthenticationTicket value);
            return Task.FromResult(value);
        }

        /// <summary></summary>
        /// <param name="ticket"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<string> StoreAsync(AuthenticationTicket ticket)
        {
            var key = $"{KeyPrefix}{Guid.NewGuid():N}";
            await RenewAsync(key, ticket);
            return key;
        }
    }
}