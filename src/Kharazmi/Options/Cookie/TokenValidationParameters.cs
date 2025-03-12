using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace Kharazmi.Options.Cookie
{
    public sealed class TokenValidationParameters : NestedOption
    {
        public TokenValidationParameters()
        {
            AuthenticationType = "AuthenticationTypes.Federation";
            RequireExpirationTime = true;
            RequireSignedTokens = true;
            RequireAudience = true;
            SaveSigninToken = false;
            TryAllIssuerSigningKeys = true;
            ValidateActor = false;
            ValidateAudience = true;
            ValidateIssuer = true;
            ValidateIssuerSigningKey = false;
            ValidateLifetime = true;
            ValidateTokenReplay = false;
            NameClaimType = ClaimsIdentity.DefaultNameClaimType;
            RoleClaimType = ClaimsIdentity.DefaultRoleClaimType;
            IgnoreTrailingSlashWhenValidatingAudience = true;
            ClockSkew = TimeSpan.FromSeconds(300);
        }

        public string NameClaimType { get; set; }
        public string RoleClaimType { get; set; }
        public TokenValidationParameters? ActorValidationParameters { get; set; }
        public string AuthenticationType { get; set; }
        public TimeSpan ClockSkew { get; set; }
        public bool IgnoreTrailingSlashWhenValidatingAudience { get; set; }
        public IDictionary<string, Object>? PropertyBag { get; set; }
        public bool RequireExpirationTime { get; set; }
        public bool RequireSignedTokens { get; set; }
        public bool SaveSigninToken { get; set; }
        public bool TryAllIssuerSigningKeys { get; set; }
        public bool ValidateActor { get; set; }
        public bool RequireAudience { get; set; }
        public bool ValidateAudience { get; set; }
        public string? ValidAudience { get; set; }
        public IEnumerable<string>? ValidAudiences { get; set; }
        public bool ValidateIssuer { get; set; }
        public string? ValidIssuer { get; set; }
        public IEnumerable<string>? ValidIssuers { get; set; }
        public bool ValidateIssuerSigningKey { get; set; }

        public bool ValidateLifetime { get; set; }
        public bool ValidateTokenReplay { get; set; }
        public IEnumerable<string>? ValidAlgorithms { get; set; }

        public IEnumerable<string>? ValidTypes { get; set; }

        public override void Validate()
        {
        }
    }
}