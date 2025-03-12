#region

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using Kharazmi.Claims;
using Kharazmi.Extensions;
using Kharazmi.Guard;

#endregion

namespace Kharazmi.Mvc.Extensions
{
    public static class UserContextExtensions
    {
        private static string? FindFirstValue([NotNull] this ClaimsPrincipal principal, string claimType)
        {
            if (claimType.IsEmpty()) return "";
            principal.NotNull(nameof(principal));
            var claim = principal.FindFirst(claimType);
            return claim?.Value;
        }

        private static string? FindFirstValue([NotNull] this ClaimsIdentity identity, string claimType)
        {
            return identity.FindFirst(claimType)?.Value;
        }

        public static string? FindFirstValue([NotNull] this IIdentity identity, string claimType)
        {
            var identity1 = identity as ClaimsIdentity;
            return identity1?.FindFirst(claimType)?.Value;
        }


        public static T? FindUserId<T>([NotNull] this IIdentity identity) where T : IEquatable<T>
        {
            var userId = identity.FindUserId();
            return userId.IsNotEmpty() ? userId.FromString<T>() : default;
        }

        public static string? FindUserId([NotNull] this IIdentity identity)
        {
            if (!(identity is ClaimsIdentity claimsIdentity))
                return null;
            var value = claimsIdentity.FindFirstValue(ClaimTypeHelper.IdentityTypes.NameIdentifier);
            if (value.IsEmpty()) value = claimsIdentity.FindFirstValue(ClaimTypeHelper.JwtTypes.Subject);
            return value;
        }

        public static string? FindUserDisplayName([NotNull] this IIdentity identity)
        {
            if (!(identity is ClaimsIdentity claimsIdentity))
                return null;
            var displayName = $"{claimsIdentity.FindFirstName()} {claimsIdentity.FindLastName()}";
            return displayName.IsEmpty() ? claimsIdentity.FindUserName() : displayName;
        }

        public static string? FindUserName([NotNull] this IIdentity identity)
        {
            if (!(identity is ClaimsIdentity claimsIdentity))
                return null;
            var userName = claimsIdentity.FindFirstValue(ClaimTypeHelper.IdentityTypes.Name);
            if (userName.IsEmpty()) userName = claimsIdentity.FindFirstValue(ClaimTypeHelper.JwtTypes.Name);
            if (userName.IsEmpty()) userName = claimsIdentity.Name;
            return userName;
        }

        public static string? FindUserEmail([NotNull] this IIdentity identity)
        {
            if (!(identity is ClaimsIdentity claimsIdentity))
                return null;
            var email = claimsIdentity.FindFirstValue(ClaimTypeHelper.IdentityTypes.Email);
            if (email.IsEmpty()) email = claimsIdentity.FindFirstValue(ClaimTypeHelper.JwtTypes.Email);
            return email;
        }

        public static string? FindFirstName([NotNull] this IIdentity identity)
        {
            if (!(identity is ClaimsIdentity claimsIdentity))
                return null;
            var firstName = claimsIdentity.FindFirstValue(ClaimTypeHelper.IdentityTypes.GivenName);
            if (firstName.IsEmpty()) firstName = claimsIdentity.FindFirstValue(ClaimTypeHelper.JwtTypes.GivenName);
            return firstName;
        }

        public static string? FindLastName([NotNull] this IIdentity identity)
        {
            if (!(identity is ClaimsIdentity claimsIdentity))
                return null;
            var firstName = claimsIdentity.FindFirstValue(ClaimTypeHelper.IdentityTypes.Surname);
            if (firstName.IsEmpty()) firstName = claimsIdentity.FindFirstValue(ClaimTypeHelper.JwtTypes.FamilyName);
            return firstName;
        }

        public static string? FindClientId([NotNull] this ClaimsPrincipal principal)
        {
            return principal.FindFirstValue(ClaimTypeHelper.JwtTypes.ClientId);
        }

        public static string? FindUserType([NotNull] this IIdentity identity)
        {
            if (!(identity is ClaimsIdentity claimsIdentity))
                return null;
            var userType = claimsIdentity.FindFirstValue(ClaimTypeHelper.JwtTypes.UserType);
            return userType;
        }

        public static bool IsAdminClient([NotNull] this IIdentity identity)
        {
            var userType = identity.FindUserType();
            return userType?.Equals("AdminClient") ?? false;
        }

        public static IEnumerable<Claim> Get([NotNull] this IIdentity identity, Func<Claim, bool>? predication)
        {
            var claimsIdentity = identity as ClaimsIdentity;
            var claimValue = predication is null ? claimsIdentity?.Claims : claimsIdentity?.Claims.Where(predication);
            return claimValue ?? new List<Claim>();
        }

        public static IReadOnlyCollection<string> GetUserRoles([NotNull] this IIdentity identity)
        {
            var claimValue = identity.Get(x => x.Type == ClaimTypeHelper.JwtTypes.Role)
                .Select(x => x.Value);
            return claimValue.AsReadOnly();
        }

        public static IReadOnlyCollection<string> GetUserPermissions([NotNull] this IIdentity identity)
        {
            var claimValue = identity.Get(x => x.Type == ClaimTypeHelper.JwtTypes.Permissions)
                .Select(x => x.Value);
            return claimValue.AsReadOnly();
        }

        public static IReadOnlyCollection<string>? GetUserClientNames([NotNull] this IIdentity identity)
        {
            var claimsIdentity = identity as ClaimsIdentity;
            var claimValue = claimsIdentity?.FindFirstValue(ClaimTypeHelper.JwtTypes.UserClientNames);
            var userClientNames = claimValue?.Split(',');
            return userClientNames?.AsReadOnly();
        }

        public static IReadOnlyCollection<string>? GetUserClientEmails([NotNull] this IIdentity identity)
        {
            var claimsIdentity = identity as ClaimsIdentity;
            var claimValue = claimsIdentity?.FindFirstValue("client_user_email");
            var userClientNames = claimValue?.Split(',');
            return userClientNames?.AsReadOnly();
        }
    }
}