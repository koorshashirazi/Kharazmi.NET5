namespace Kharazmi.Constants
{
    public static class CookieConstants
    {
        public const string HeaderValueNoCache = "no-cache";
        public const string HeaderValueEpocDate = "Thu, 01 Jan 1970 00:00:00 GMT";
        public const string SessionIdClaim = "Microsoft.AspNetCore.Authentication.Cookies-SessionId";

        public const string CookiePrefix = ".PersianCat.";

        public const string ErrorPath = "/Error/Index";
        public const string RedirectOnCookieExpire = "/Account/Login";
    }
}