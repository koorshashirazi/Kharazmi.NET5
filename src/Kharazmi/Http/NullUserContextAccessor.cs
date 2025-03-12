using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Kharazmi.Common.ValueObjects;
using Kharazmi.Dependency;
using Kharazmi.Extensions;
using Kharazmi.Functional;
using Microsoft.AspNetCore.Http;

namespace Kharazmi.Http
{
    public class NullUserContextAccessor : IUserContextAccessor, INullInstance
    {
        public NullUserContextAccessor()
        {
        }

        public IHttpClientFactory? HttpClientFactory => default;
        public Maybe<HttpContext> HttpContext => Maybe<HttpContext>.None;
        public Maybe<HttpRequest> Request => Maybe<HttpRequest>.None;
        public Maybe<ClaimsPrincipal> CurrentUser { get; } = Maybe<ClaimsPrincipal>.None;
        public Maybe<ClaimsIdentity> CurrentIdentity { get; } = Maybe<ClaimsIdentity>.None;
        public bool IsAuthenticated { get; } = false;
        public Maybe<string> UserDisplayName { get; } = Maybe<string>.None;
        public Maybe<string> UserId { get; } = Maybe<string>.None;
        public Maybe<string> UserName { get; } = Maybe<string>.None;
        public Maybe<string> Email { get; } = Maybe<string>.None;
        public Maybe<string> UserBrowserName { get; } = Maybe<string>.None;
        public Maybe<string> UserIp { get; } = Maybe<string>.None;
        public bool IsAdminClient { get; } = false;
        public Maybe<string> TraceId { get; } = Maybe<string>.None;
        public Maybe<string> ConnectionId { get; } = Maybe<string>.None;
        public Maybe<string> RequestPath { get; } = Maybe<string>.None;
        public IReadOnlyCollection<string> Roles { get; } = Enumerable.Empty<string>().AsReadOnly();
        public IReadOnlyCollection<string> Permissions { get; } = Enumerable.Empty<string>().AsReadOnly();
        public IReadOnlyCollection<string> ClientUserNames { get; } = Enumerable.Empty<string>().AsReadOnly();
        public IReadOnlyCollection<string> ClientUserEmails { get; } = Enumerable.Empty<string>().AsReadOnly();
        public IEnumerable<Claim> Claims { get; } = Enumerable.Empty<Claim>();
        public CultureInfo GetCurrentCulture { get; } = System.Threading.Thread.CurrentThread.CurrentCulture;
        public CultureInfo GetCurrentUiCulture { get; } = System.Threading.Thread.CurrentThread.CurrentUICulture;
        public string CultureName { get; } = System.Threading.Thread.CurrentThread.CurrentCulture.Name;
        public string UiCultureName { get; } = System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
        public bool IsAdmin(string adminRoleName, string adminUserName) => false;

        public bool IsInRole(string role) => false;

        public bool HasClaim(Predicate<Claim> predicate) => false;

        public Maybe<Claim> FindClaim(string claimType) => Maybe<Claim>.None;

        public Task<Maybe<string>> CurrentAccessToken() => Task.FromResult(Maybe<string>.None);

        public Task<Maybe<string>> CurrentRefreshToken() => Task.FromResult(Maybe<string>.None);

        public Task<Result> RevokeAccessToken(DiscoveryTokenOptions options) => Task.FromResult(Result.Fail(""));

        public Task<Result> RevokeRefreshToken(DiscoveryTokenOptions options) => Task.FromResult(Result.Fail(""));
    }
}