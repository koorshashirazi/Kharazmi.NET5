using System;
using System.Threading.Tasks;
using Kharazmi.Constants;
using Kharazmi.Helpers;
using Kharazmi.Options.Cookie;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection;

namespace Kharazmi.OpenIdConnect.OpenIdConnectHandlers
{
    /// <summary>_</summary>
    public interface IMessageReceived
    {
        /// <summary>_</summary>
        /// <param name="context"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        Task HandleAsync(MessageReceivedContext context, ExtendedOpenIdOption option);
    }

    internal class DefaultMessageReceived : IMessageReceived
    {
        public Task HandleAsync(MessageReceivedContext context, ExtendedOpenIdOption option)
        {
            DateTimeOffset issuedUtc;
            if (context.Properties.IssuedUtc.HasValue)
            {
                issuedUtc = context.Properties.IssuedUtc.Value;
            }
            else
            {
                var clock = context.HttpContext?.RequestServices?.GetService<ISystemClock>();
                issuedUtc = clock?.UtcNow ?? DateTimeHelper.DateTimeOffsetUtcNow;
                context.Properties.IssuedUtc = issuedUtc;
            }

            context.Properties.RedirectUri = option.RedirectUri;
            var cookieOption = option.CookieOption;
            if (cookieOption is null) return Task.CompletedTask;

            context.Properties.IsPersistent = cookieOption.IsPersistent;

            // if (!context.Properties.IsPersistent) return Task.CompletedTask;

            var exp = cookieOption.ExpirationOptions;
            context.Properties.ExpiresUtc =
                issuedUtc.Add(exp.AbsoluteExpiration ?? ExpirationConstants.AbsoluteExpiration).ToUniversalTime();

            return Task.CompletedTask;
        }
    }
}