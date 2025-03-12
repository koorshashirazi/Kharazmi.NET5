#region

using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Threading.Tasks;
using Kharazmi.Extensions;
using Kharazmi.OpenIdConnect.OpenIdConnectHandlers;
using Kharazmi.Options.Cookie;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

#endregion


namespace Kharazmi.OpenIdConnect.Extensions
{
    /// <summary> </summary>
    internal static class OpenIdExtensions
    {
        /// <summary> </summary>
        internal static OpenIdConnectOptions ConfigureExtendOpenId(this OpenIdConnectOptions openIdOptions,
            [NotNull] ExtendedOpenIdOption option,
            Microsoft.IdentityModel.Tokens.TokenValidationParameters? extendedOption = null)
        {
            openIdOptions.Authority = option.AuthorityUrl;
            openIdOptions.ResponseType = option.ResponseType;
            openIdOptions.ResponseMode = option.ResponseMode;
            openIdOptions.RequireHttpsMetadata = option.RequireHttpsMetadata;
            openIdOptions.ClientId = option.ClientId;
            openIdOptions.ClientSecret = option.ClientSecret;
            openIdOptions.NonceCookie.SameSite = SameSiteMode.None;

            if (option.AlwaysLoginPrompt)
                openIdOptions.Prompt = "login";

            if (option.AllowedScopes != null)
                foreach (var scope in option.AllowedScopes)
                    openIdOptions.Scope.Add(scope);

            openIdOptions.SaveTokens = option.SaveTokens;
            openIdOptions.GetClaimsFromUserInfoEndpoint = option.GetClaimsFromUserInfoEndpoint;

            if (option.ClaimsForRemove?.Length > 0)
                foreach (var claim in option.ClaimsForRemove)
                    openIdOptions.ClaimActions.Remove(claim);

            if (option.RequiredClaims != null)
                foreach (var requiredClaim in option.RequiredClaims)
                    openIdOptions.ClaimActions.MapJsonKey(requiredClaim, requiredClaim);

            var tokenParameters =
                option.TokenValidationParameters.MapTo<Microsoft.IdentityModel.Tokens.TokenValidationParameters>() ??
                new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    NameClaimType = ClaimsIdentity.DefaultNameClaimType,
                    RoleClaimType = ClaimsIdentity.DefaultRoleClaimType
                };

            if (extendedOption is not null)
                tokenParameters.Merge(extendedOption);

            openIdOptions.TokenValidationParameters = tokenParameters;

            openIdOptions.Events = new OpenIdConnectEvents
            {
                OnMessageReceived = messageContext => OnMessageReceived(messageContext, option),
                OnRedirectToIdentityProvider =
                    redirectContext => OnRedirectToIdentityProvider(redirectContext, option),
                OnTokenValidated = OnTokenValidated,
                OnUserInformationReceived = OnUserInformationReceived,
                OnRedirectToIdentityProviderForSignOut =
                    OnRedirectToIdentityProviderForSignOut,
                OnRemoteFailure = OnRemoteFailure,
            };

            return openIdOptions;
        }

        private static Task OnRemoteFailure(RemoteFailureContext context)
        {
            var handlers = context.HttpContext.RequestServices.GetService<IRemoteFailureHandler>();
            return handlers?.HandleAsync(context) ?? Task.CompletedTask;
        }

        private static Task OnUserInformationReceived(UserInformationReceivedContext context)
        {
            var handlers = context.HttpContext.RequestServices.GetService<IUserInformationReceivedHandler>();
            return handlers?.HandleAsync(context) ?? Task.CompletedTask;
        }

        private static Task OnRedirectToIdentityProviderForSignOut(RedirectContext context)
        {
            var handlers = context.HttpContext.RequestServices.GetService<IRedirectToIdentityProviderForSignOut>();
            return handlers?.HandleAsync(context) ?? Task.CompletedTask;
        }

        private static Task OnTokenValidated(TokenValidatedContext context)
        {
            var handlers = context.HttpContext.RequestServices.GetService<ITokenValidated>();
            return handlers?.HandleAsync(context) ?? Task.CompletedTask;
        }

        private static Task OnMessageReceived(MessageReceivedContext context, ExtendedOpenIdOption option)
        {
            var handlers = context.HttpContext.RequestServices.GetService<IMessageReceived>();
            return handlers?.HandleAsync(context, option) ?? Task.CompletedTask;
        }

        private static Task OnRedirectToIdentityProvider(RedirectContext context, ExtendedOpenIdOption option)
        {
            var handlers = context.HttpContext.RequestServices.GetService<IRedirectToIdentity>();
            return handlers?.HandleAsync(context, option) ?? Task.CompletedTask;
        }
    }
}