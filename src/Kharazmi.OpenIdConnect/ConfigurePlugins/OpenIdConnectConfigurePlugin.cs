using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using Kharazmi.Configuration;
using Kharazmi.ConfigurePlugins;
using Kharazmi.Extensions;
using Kharazmi.OpenIdConnect.CookieHandlers;
using Kharazmi.OpenIdConnect.Default;
using Kharazmi.OpenIdConnect.Extensions;
using Kharazmi.OpenIdConnect.OpenIdConnectHandlers;
using Kharazmi.Options.Cookie;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using CookieAuthenticationHandler = Kharazmi.OpenIdConnect.CookieHandlers.CookieAuthenticationHandler;

namespace Kharazmi.OpenIdConnect.ConfigurePlugins
{
    internal class OpenIdConnectConfigurePlugin : IConfigurePlugin
    {
        public string PluginName { get; }

        public void Configure(ISettingProvider settings)
        {
            var option = settings.Get<ExtendedCookieOption>();

            settings.UpdateOption(option);

            var openIdOption = settings.Get<ExtendedOpenIdOption>();

            settings.UpdateOption(openIdOption);
        }

        public void Initialize(IServiceCollection services, ISettingProvider settings)
        {
            var option = settings.Get<ExtendedCookieOption>();
            var openIdOption = settings.Get<ExtendedOpenIdOption>();
            if (option.UseDisabledAuthorization)
                services.ReplaceService<IPolicyEvaluator, DisabledAuthorization>(ServiceLifetime.Singleton);
            
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap = new Dictionary<string, string>();

            AuthenticationBuilder? authenticationBuilder = null;
            if (option.UseAuthenticationCookie)
            {
                authenticationBuilder = services.AddAuthentication(config =>
                    {
                        config.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                        config.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                        config.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                        config.DefaultForbidScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                        config.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                        config.DefaultSignOutScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    })
                    .AddScheme<ExtendedCookieAuthenticationOptions, CookieAuthenticationHandler>(
                        CookieAuthenticationDefaults.AuthenticationScheme, "AuthScheme",
                        config => config.ConfigureExtendedCookieAuthentication(option));

                // PostConfigureCookieAuthenticationOptions
                services.TryAddEnumerable(ServiceDescriptor
                    .Singleton<IPostConfigureOptions<ExtendedCookieAuthenticationOptions>,
                        CustomPostConfigureCookie>());

                services.AddTransient<IClaimsTransformation, OpenIdClaimsTransformation>();
                services.AddTransient<ICookieValidationHandler, CookieValidationHandler>();
                services.AddTransient<ICookieUnauthorizedRequestHandler, CookieUnauthorizedRequestHandler>();
                services.AddTransient<ICookieSignedIn, NullCookieSignedIn>();
                services.AddTransient<ICookieSigningIn, NullCookieSigningIn>();
                services.AddTransient<ICookieSigningOut, NullCookieSigningOut>();
                services.AddTransient<ICookieRedirectToLogout, NullICookieRedirectToLogout>();
                services.AddTransient<ICookieRedirectToAccessDenied, NullCookieRedirectToAccessDenied>();
                services.AddTransient<ICookieRedirectToReturnUrl, NullCookieRedirectToReturnUrl>();
            }

            if (!openIdOption.Enable || authenticationBuilder is null) return;
            
            authenticationBuilder.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme,
                config => config.ConfigureExtendOpenId(openIdOption));

            services.AddTransient<IRemoteFailureHandler, RemoteFailureHandler>();
            services.AddTransient<IMessageReceived, DefaultMessageReceived>();
            services.AddTransient<IRedirectToIdentity, DefaultRedirectToIdentity>();
            services
                .AddTransient<IRedirectToIdentityProviderForSignOut, NullRedirectToIdentityProviderForSignOut>();
            services.AddTransient<ITokenValidated, NullTokenValidated>();
            services.AddTransient<IUserInformationReceivedHandler, NullUserInformationReceivedHandler>();

        }
    }
}