#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Claims;
using System.Threading.Tasks;
using Kharazmi.Common.ValueObjects;
using Kharazmi.Dependency;
using Kharazmi.Functional;
using Microsoft.AspNetCore.Http;

#endregion

namespace Kharazmi.Http
{
    public interface IUserContextAccessor : IShouldBeSingleton, IMustBeInstance
    {
        IHttpClientFactory? HttpClientFactory { get; }
        Maybe<HttpContext> HttpContext { get; }
        Maybe<HttpRequest> Request { get; }
        Maybe<ClaimsPrincipal> CurrentUser { get; }
        Maybe<ClaimsIdentity> CurrentIdentity { get; }
        bool IsAuthenticated { get; }
        Maybe<string> UserDisplayName { get; }
        Maybe<string> UserId { get; }
        Maybe<string> UserName { get; }
        Maybe<string> Email { get; }
        Maybe<string> UserBrowserName { get; }
        Maybe<string> UserIp { get; }
        bool IsAdminClient { get; }
        Maybe<string> TraceId { get; }
        Maybe<string> ConnectionId { get; }
        Maybe<string> RequestPath { get; }
        IReadOnlyCollection<string> Roles { get; }
        IReadOnlyCollection<string> Permissions { get; }
        IReadOnlyCollection<string> ClientUserNames { get; }
        IReadOnlyCollection<string> ClientUserEmails { get; }
        IEnumerable<Claim> Claims { get; }
        CultureInfo GetCurrentCulture { get; }
        CultureInfo GetCurrentUiCulture { get; }
        string CultureName { get; }
        string UiCultureName { get; }
        bool IsAdmin(string adminRoleName, string adminUserName);
        bool IsInRole(string role);
        bool HasClaim(Predicate<Claim> predicate);
        Maybe<Claim> FindClaim(string claimType);
        Task<Maybe<string>> CurrentAccessToken();
        Task<Maybe<string>> CurrentRefreshToken();
        Task<Result> RevokeAccessToken(DiscoveryTokenOptions options);
        Task<Result> RevokeRefreshToken(DiscoveryTokenOptions options);
    }
}