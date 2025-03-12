#region

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Kharazmi.Constants;
using Kharazmi.Extensions;
using Kharazmi.Guard;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

#endregion

namespace Kharazmi.OpenIdConnect.CookieHandlers
{
    /// <summary> </summary>
    internal class CookieAuthenticationHandler : SignInAuthenticationHandler<ExtendedCookieAuthenticationOptions>
    {
        private bool _shouldRefresh;
        private bool _signInCalled;
        private bool _signOutCalled;
        private DateTimeOffset? _refreshIssuedUtc;
        private DateTimeOffset? _refreshExpiresUtc;
        private string? _sessionKey;
        private Task<AuthenticateResult>? _readCookieTask;
        private AuthenticationTicket? _refreshTicket;

        /// <summary>_</summary>
        /// <param name="options"></param>
        /// <param name="logger"></param>
        /// <param name="encoder"></param>
        /// <param name="clock"></param>
        public CookieAuthenticationHandler(
            IOptionsMonitor<ExtendedCookieAuthenticationOptions> options,
            [AllowNull] ILoggerFactory? logger,
            UrlEncoder encoder,
            ISystemClock clock)
            : base(options, logger ?? NullLoggerFactory.Instance, encoder, clock)
        {
        }


        /// <summary>_</summary>
        public virtual string SessionIdClaimType => string.IsNullOrWhiteSpace(Options.SessionIdClaim)
            ? CookieConstants.SessionIdClaim
            : Options.SessionIdClaim;

        /// <summary> </summary>
        protected new CookieAuthenticationEvents? Events
        {
            get => base.Events as CookieAuthenticationEvents;
            set => base.Events = value;
        }

        /// <summary>_</summary>
        /// <returns></returns>
        protected override Task InitializeHandlerAsync()
        {
            // Cookies needs to finish the response
            Context.Response.OnStarting(FinishResponseAsync);
            return Task.CompletedTask;
        }

        /// <summary>_</summary>
        /// <returns></returns>
        protected override Task<object> CreateEventsAsync()
            => Task.FromResult<object>(new CookieAuthenticationEvents());

        /// <summary>_</summary>
        /// <returns></returns>
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var result = await EnsureCookieTicket();
            if (!result.Succeeded) return result;

            var context = new CookieValidatePrincipalContext(Context, Scheme, Options, result.Ticket);

            if (Events != null) await Events.ValidatePrincipal(context);

            if (context.Principal is null) return AuthenticateResult.Fail("No principal.");

            if (context.ShouldRenew) RequestRefresh(result.Ticket, context.Principal);

            return AuthenticateResult.Success(new AuthenticationTicket(context.Principal, context.Properties,
                Scheme.Name));
        }

        /// <summary>_</summary>
        /// <returns></returns>
        protected virtual async Task FinishResponseAsync()
        {
            // Only renew if requested, and neither sign in or sign out was called
            if (!_shouldRefresh || _signInCalled || _signOutCalled) return;

            var ticket = _refreshTicket;
            if (ticket != null)
            {
                var properties = ticket.Properties;

                if (_refreshIssuedUtc.HasValue) properties.IssuedUtc = _refreshIssuedUtc;

                if (_refreshExpiresUtc.HasValue) properties.ExpiresUtc = _refreshExpiresUtc;

                if (Options.SessionStore != null && _sessionKey != null)
                {
                    await Options.SessionStore.RenewAsync(_sessionKey, ticket);
                    var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
                        {
                            new Claim(SessionIdClaimType, _sessionKey, ClaimValueTypes.String, Options.ClaimsIssuer)
                        },
                        Scheme.Name));
                    ticket = new AuthenticationTicket(principal, null, Scheme.Name);
                }

                var token = EncodeToken();
                var cookieValue = Options.TicketDataFormat.Protect(ticket, token);
                var cookieOptions = BuildCookieOptions();

                if (properties.IsPersistent && _refreshExpiresUtc.HasValue)
                    cookieOptions.Expires = _refreshExpiresUtc.Value;


                Options.CookieManager.AppendResponseCookie(Context, Options.Cookie.Name!, cookieValue, cookieOptions);

                await ApplyHeaders(false, properties);
            }
        }

        /// <summary>_</summary>
        /// <param name="claimsPrincipal"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        protected override async Task HandleSignInAsync(ClaimsPrincipal claimsPrincipal,
            AuthenticationProperties? properties)
        {
            claimsPrincipal = claimsPrincipal.NotNull(nameof(claimsPrincipal));
            properties ??= new AuthenticationProperties();

            _signInCalled = true;

            var result = await EnsureCookieTicket();
            var unprotectedTicket = result.Ticket;
            var cookieOptions = BuildCookieOptions();

            DateTimeOffset issuedUtc;
            if (properties.IssuedUtc.HasValue)
            {
                issuedUtc = properties.IssuedUtc.Value;
            }
            else
            {
                issuedUtc = Clock.UtcNow;
                properties.IssuedUtc = issuedUtc;
            }

            var expiresUtc = issuedUtc.Add(Options.ExpireTimeSpan);

            if (properties.IsPersistent)
                cookieOptions.Expires = expiresUtc;

            properties.ExpiresUtc = expiresUtc;

            var signInContext =
                new CookieSigningInContext(Context, Scheme, Options, claimsPrincipal, properties, cookieOptions);

            if (Events != null)
                await Events.SigningIn(signInContext);

            AuthenticationTicket ticket;

            if (result.Succeeded && unprotectedTicket is not null)
            {
                ticket = new AuthenticationTicket(unprotectedTicket.Principal, unprotectedTicket.Properties,
                    unprotectedTicket.AuthenticationScheme);
            }
            else
            {
                ticket = new AuthenticationTicket(claimsPrincipal, properties, signInContext.Scheme.Name);
            }


            // Store th ticket and create new ticket with only _sessionKey
            if (Options.SessionStore != null)
            {
                if (_sessionKey != null)
                {
                    // Renew ticket
                    await Options.SessionStore.RenewAsync(_sessionKey, ticket);

                    // Create new ticket only with session id
                    ticket = CreateAuthenticationTicketWithSession(_sessionKey);
                }
                else
                {
                    _sessionKey = await Options.SessionStore.StoreAsync(ticket);
                    ticket = CreateAuthenticationTicketWithSession(_sessionKey);
                }
            }

            var token = EncodeToken();
            var cookieValue = Options.TicketDataFormat.Protect(ticket, token);

            Options.CookieManager.AppendResponseCookie(Context, Options.Cookie.Name, cookieValue,
                signInContext.CookieOptions);

            var signedInContext = new CookieSignedInContext(Context, Scheme, claimsPrincipal, properties, Options);

            if (Events != null)
                await Events.SignedIn(signedInContext);

            var shouldRedirect = Options.LoginPath.HasValue && OriginalPath == Options.LoginPath;

            await ApplyHeaders(shouldRedirect, properties);

            Logger?.LogTrace("AuthenticationSchemeSignedIn: {Name} was successfully authenticated", Scheme.Name);
        }

        private AuthenticationTicket CreateAuthenticationTicketWithSession(string sessionKey)
        {
            return new(new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(SessionIdClaimType, sessionKey, ClaimValueTypes.String, Options.ClaimsIssuer)
            }, Options.ClaimsIssuer)), null, Scheme.Name);
        }

        /// <summary>_</summary>
        /// <param name="properties"></param>
        /// <returns></returns>
        protected override async Task HandleSignOutAsync(AuthenticationProperties? properties)
        {
            properties ??= new AuthenticationProperties();

            _signOutCalled = true;

            await EnsureCookieTicket();
            var cookieOptions = BuildCookieOptions();
            if (Options.SessionStore != null && _sessionKey != null)
                await Options.SessionStore.RemoveAsync(_sessionKey);

            var context = new CookieSigningOutContext(Context, Scheme, Options, properties, cookieOptions);

            if (Events != null) await Events.SigningOut(context);

            Options.CookieManager.DeleteCookie(
                Context,
                Options.Cookie.Name!,
                context.CookieOptions);

            var shouldRedirect = Options.LogoutPath.HasValue && OriginalPath == Options.LogoutPath;
            await ApplyHeaders(shouldRedirect, context.Properties);

            Logger?.LogTrace("AuthenticationSchemeSignedOut: {Name} was successfully authenticated", Scheme.Name);
        }

        /// <summary>_</summary>
        /// <param name="properties"></param>
        /// <returns></returns>
        protected override async Task HandleForbiddenAsync(AuthenticationProperties properties)
        {
            var returnUrl = properties.RedirectUri;
            if (string.IsNullOrEmpty(returnUrl)) returnUrl = OriginalPathBase + OriginalPath + Request.QueryString;

            var accessDeniedUri = Options.AccessDeniedPath + QueryString.Create(Options.ReturnUrlParameter, returnUrl);
            var redirectContext = new RedirectContext<CookieAuthenticationOptions>(Context, Scheme, Options, properties,
                BuildRedirectUri(accessDeniedUri));
            if (Events != null) await Events.RedirectToAccessDenied(redirectContext);
        }

        /// <summary>_</summary>
        /// <param name="properties"></param>
        /// <returns></returns>
        protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            var redirectUri = properties.RedirectUri;
            if (string.IsNullOrEmpty(redirectUri)) redirectUri = OriginalPathBase + OriginalPath + Request.QueryString;

            var loginUri = Options.LoginPath + QueryString.Create(Options.ReturnUrlParameter, redirectUri);
            var redirectContext = new RedirectContext<CookieAuthenticationOptions>(Context, Scheme, Options, properties,
                BuildRedirectUri(loginUri));
            if (Events != null) await Events.RedirectToLogin(redirectContext);
        }

        #region Helpers Method

        private async Task ApplyHeaders(bool shouldRedirectToReturnUrl, AuthenticationProperties properties)
        {
            Response.Headers[HeaderNames.CacheControl] = CookieConstants.HeaderValueNoCache;
            Response.Headers[HeaderNames.Pragma] = CookieConstants.HeaderValueNoCache;
            Response.Headers[HeaderNames.Expires] = CookieConstants.HeaderValueEpocDate;

            if (shouldRedirectToReturnUrl && Response.StatusCode == 200)
            {
                var redirectUri = properties.RedirectUri;
                if (redirectUri.IsEmpty())
                {
                    redirectUri = Request.Query[Options.ReturnUrlParameter];
                    if (redirectUri.IsEmpty() || !IsHostRelative(redirectUri)) redirectUri = null;
                }

                if (redirectUri != null)
                    if (Events != null)
                        await Events.RedirectToReturnUrl(
                            new RedirectContext<CookieAuthenticationOptions>(Context, Scheme, Options, properties,
                                redirectUri));
            }
        }

        private static bool IsHostRelative(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;

            if (path.Length == 1) return path[0] == '/';

            return path[0] == '/' && path[1] != '/' && path[1] != '\\';
        }

        private string? EncodeToken()
        {
            var binding = Context.Features?.Get<ITlsTokenBindingFeature>()?.GetProvidedTokenBindingId();
            return binding != null ? Convert.ToBase64String(binding) : null;
        }

        private Task<AuthenticateResult> EnsureCookieTicket()
        {
            return _readCookieTask ??= ReadCookieTicket();
        }

        private void CheckForRefresh(AuthenticationTicket ticket)
        {
            var currentUtc = Clock.UtcNow;
            var issuedUtc = ticket.Properties.IssuedUtc;
            var expiresUtc = ticket.Properties.ExpiresUtc;
            var allowRefresh = ticket.Properties.AllowRefresh ?? true;
            if (issuedUtc != null && expiresUtc != null && Options.SlidingExpiration && allowRefresh)
            {
                var timeElapsed = currentUtc.Subtract(issuedUtc.Value);
                var timeRemaining = expiresUtc.Value.Subtract(currentUtc);

                if (timeRemaining < timeElapsed) RequestRefresh(ticket);
            }
        }

        private void RequestRefresh(AuthenticationTicket ticket, ClaimsPrincipal? replacedPrincipal = null)
        {
            var issuedUtc = ticket.Properties.IssuedUtc;
            var expiresUtc = ticket.Properties.ExpiresUtc;

            if (issuedUtc != null && expiresUtc != null)
            {
                _shouldRefresh = true;
                var currentUtc = Clock.UtcNow;
                _refreshIssuedUtc = currentUtc;
                var timeSpan = expiresUtc.Value.Subtract(issuedUtc.Value);
                _refreshExpiresUtc = currentUtc.Add(timeSpan);
                if (replacedPrincipal != null) _refreshTicket = CloneTicket(ticket, replacedPrincipal);
            }
        }

        private AuthenticationTicket CloneTicket(AuthenticationTicket ticket, ClaimsPrincipal? replacedPrincipal)
        {
            var principal = replacedPrincipal ?? ticket.Principal;
            var newPrincipal = new ClaimsPrincipal();
            foreach (var identity in principal.Identities) newPrincipal.AddIdentity(identity.Clone());

            var newProperties = new AuthenticationProperties();
            foreach (var item in ticket.Properties.Items) newProperties.Items[item.Key] = item.Value;

            return new AuthenticationTicket(newPrincipal, newProperties, ticket.AuthenticationScheme);
        }

        private async Task<AuthenticateResult> ReadCookieTicket()
        {
            var cookieName = Options.Cookie.Name;
            if (cookieName.IsEmpty())
                return AuthenticateResult.NoResult();

            var cookie = Options.CookieManager.GetRequestCookie(Context, cookieName);
            if (cookie.IsEmpty()) return AuthenticateResult.NoResult();

            var token = EncodeToken();

            var ticket = Options.TicketDataFormat.Unprotect(cookie, token);
            if (ticket is null) return AuthenticateResult.Fail("Unprotect ticket failed");

            if (Options.SessionStore != null)
            {
                var claim = ticket.Principal.Claims.FirstOrDefault(c => c.Type.Equals(SessionIdClaimType));
                if (claim is null) return AuthenticateResult.Fail("SessionId missing");

                _sessionKey = claim.Value;
                ticket = await Options.SessionStore.RetrieveAsync(_sessionKey);
            }

            var currentUtc = Clock.UtcNow;
            var expiresUtc = ticket.Properties.ExpiresUtc;

            if (expiresUtc != null && expiresUtc.Value < currentUtc)
            {
                if (Options.SessionStore != null && !string.IsNullOrWhiteSpace(_sessionKey))
                    await Options.SessionStore.RemoveAsync(_sessionKey);

                return AuthenticateResult.Fail("Ticket expired");
            }

            CheckForRefresh(ticket);

            var p = ticket.Principal;
            if (p.Identity?.IsAuthenticated == false || ticket.AuthenticationScheme.IsEmpty() ||
                ticket.Principal.Claims?.Any() == false)
                return AuthenticateResult.Fail("Invalid authentication ticket");
            
            return AuthenticateResult.Success(ticket);
        }

        private CookieOptions BuildCookieOptions()
        {
            var cookieOptions = Options.Cookie.Build(Context);
            cookieOptions.Expires = null;
            return cookieOptions;
        }

        #endregion Helpers Method
    }
}