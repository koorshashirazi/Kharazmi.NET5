#region

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Kharazmi.Constants;
using Kharazmi.Extensions;
using Kharazmi.HealthChecks;
using Kharazmi.Options.HealthCheck;

#endregion

namespace Kharazmi.Options.Cookie
{
    /// <summary> </summary>
    public class ExtendedOpenIdOption : ConfigurePluginOption, IHaveHealthCheckOption
    {
        private readonly HashSet<string> _allowedScopes;
        private readonly HashSet<string> _requiredClaims;
        private readonly HashSet<string> _claimsForRemove;


        /// <summary> </summary>
        public ExtendedOpenIdOption()
        {
            _allowedScopes = new HashSet<string>();
            _requiredClaims = new HashSet<string>();
            _claimsForRemove = new HashSet<string>();
        }


        /// <summary></summary>
        public bool AlwaysLoginPrompt { get; set; } = true;

        /// <summary></summary>
        [StringLength(100)]
        public string DiscoverEndpoint { get; set; } = "/.well-known/openid-configuration";

        /// <summary> </summary>
        [StringLength(100)]
        public string? AuthorityUrl { get; set; }

        /// <summary> </summary>
        [StringLength(100)]
        public string ResponseType { get; set; } = "code id_token token"; // OpenIdConnectResponseType.CodeIdTokenToken;

        /// <summary> </summary>
        [StringLength(100)]
        public string ResponseMode { get; set; } = "form_post";

        /// <summary> </summary>
        public bool RequireHttpsMetadata { get; set; } = false;

        /// <summary> </summary>
        [StringLength(100)]
        public string? ClientId { get; set; }

        /// <summary> </summary>
        [StringLength(100)]
        public string? ClientSecret { get; set; }

        /// <summary> </summary>
        [StringLength(100)]
        public string? ClientUri { get; set; }

        /// <summary> </summary>
        [StringLength(100)]
        public string? RedirectUri { get; set; }

        /// <summary> </summary>
        [StringLength(100)]
        public string? PostLogoutRedirectUris { get; set; }

        /// <summary> </summary>
        [StringLength(100)]
        public string? FrontChannelLogoutUri { get; set; }

        public bool SaveTokens { get; set; }
        public bool GetClaimsFromUserInfoEndpoint { get; set; }

        /// <summary> </summary>
        public string[]? AllowedScopes
        {
            get => _allowedScopes.ToArray();
            set
            {
                if (value is null || value.Length <= 0) return;
                foreach (var val in value)
                    _allowedScopes.Add(val);
            }
        }

        /// <summary> </summary>
        public string[]? RequiredClaims
        {
            get => _requiredClaims.ToArray();
            set
            {
                if (value is null || value.Length <= 0) return;
                foreach (var val in value)
                    _requiredClaims.Add(val);
            }
        }

        /// <summary> </summary>
        public string[]? ClaimsForRemove
        {
            get => _claimsForRemove.ToArray();
            set
            {
                if (value is null || value.Length <= 0) return;
                foreach (var val in value)
                    _claimsForRemove.Add(val);
            }
        }

        public bool UseTokenValidationParameters { get; set; }
        public TokenValidationParameters? TokenValidationParameters { get; set; }

        public bool UseCookie { get; set; }
        public CookieOption? CookieOption { get; set; }

        public bool UseHealthCheck { get; set; }
        public HealthCheckOption? HealthCheckOption { get; set; }

        public override void Validate()
        {
            if (DiscoverEndpoint.IsEmpty())
                AddValidation(MessageHelper.NullOrEmpty(MessageEventName.OptionsValidation,
                    nameof(ExtendedOpenIdOption), nameof(DiscoverEndpoint)));

            if (RedirectUri.IsEmpty())
                AddValidation(MessageHelper.NullOrEmpty(MessageEventName.OptionsValidation,
                    nameof(ExtendedOpenIdOption), nameof(RedirectUri)));

            if (ClientId.IsEmpty())
                AddValidation(MessageHelper.NullOrEmpty(MessageEventName.OptionsValidation,
                    nameof(ExtendedOpenIdOption), nameof(ClientId)));

            if (ClientSecret.IsEmpty())
                AddValidation(MessageHelper.NullOrEmpty(MessageEventName.OptionsValidation,
                    nameof(ExtendedOpenIdOption), nameof(ClientSecret)));

            if (AuthorityUrl.IsEmpty())
                AddValidation(MessageHelper.NullOrEmpty(MessageEventName.OptionsValidation,
                    nameof(ExtendedOpenIdOption), nameof(AuthorityUrl)));

            if (ResponseType.IsEmpty())
                AddValidation(MessageHelper.NullOrEmpty(MessageEventName.OptionsValidation,
                    nameof(ExtendedOpenIdOption), nameof(ResponseType)));

            if (ResponseMode.IsEmpty())
                AddValidation(MessageHelper.NullOrEmpty(MessageEventName.OptionsValidation,
                    nameof(ExtendedOpenIdOption), nameof(ResponseMode)));

            if (UseTokenValidationParameters)
            {
                TokenValidationParameters ??= new TokenValidationParameters();
                TokenValidationParameters.Validate();
            }

            if (UseCookie)
            {
                CookieOption ??= new CookieOption();
                CookieOption.Validate();
            }

            if (UseHealthCheck == false) return;
            HealthCheckOption ??= new HealthCheckOption();
            HealthCheckOption.Validate();
        }
    }
}