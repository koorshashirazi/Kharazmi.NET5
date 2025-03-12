#region

using Microsoft.AspNetCore.Authentication.Cookies;

#endregion

namespace Kharazmi.OpenIdConnect
{
    /// <summary> </summary>
    public class ExtendedCookieAuthenticationOptions : CookieAuthenticationOptions
    {
        /// <summary>_</summary>
        public string SessionIdClaim { get; set; } = "Microsoft.Owin.Security.Cookies-SessionId";
    }
}