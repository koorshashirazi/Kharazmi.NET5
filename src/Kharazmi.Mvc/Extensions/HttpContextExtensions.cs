#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kharazmi.Extensions;
using Kharazmi.Guard;
using Kharazmi.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Newtonsoft.Json;

#endregion

namespace Kharazmi.Mvc.Extensions
{
    public static class HttpContextExtensions
    {
        public static string GetHeaderValue(this HttpContext httpContext, string headerName)
        {
            httpContext.RequestSanityCheck();

            return httpContext.Request.Headers.TryGetValue(headerName, out var values)
                ? values.ToString()
                : string.Empty;
        }

        public static string FindUserAgent(this HttpContext httpContext)
        {
            return GetHeaderValue(httpContext, "User-Agent");
        }


        public static Uri GetBaseUri(this HttpContext httpContext)
        {
            return new Uri(GetBaseUrl(httpContext));
        }

        public static string GetBaseUrl(this HttpContext httpContext)
        {
            RequestSanityCheck(httpContext);
            var request = httpContext.Request;
            return $"{request.Scheme}://{request.Host.ToUriComponent()}";
        }

        public static string GetRawUrl(this HttpContext httpContext)
        {
            RequestSanityCheck(httpContext);
            return httpContext.Request.GetDisplayUrl();
        }

        public static Uri GetRawUri(this HttpContext httpContext)
        {
            return new Uri(GetRawUrl(httpContext));
        }


        public static string GetReferrerUrl(this HttpContext httpContext)
        {
            return httpContext.GetHeaderValue("Referer");
        }

        public static Uri? GetReferrerUri(this HttpContext httpContext)
        {
            var referrer = GetReferrerUrl(httpContext);
            if (referrer.IsEmpty()) return null;

            return Uri.TryCreate(referrer, UriKind.Absolute, out var result) ? result : null;
        }

        public static string? FindUserIp(this HttpContext? httpContext, bool tryUseXForwardHeader = true)
        {
            if (httpContext is null)
                return null;

            var ip = string.Empty;

            if (tryUseXForwardHeader) ip = SplitCsv(GetHeaderValue(httpContext, "X-Forwarded-For")).FirstOrDefault();

            if (ip.IsEmpty() &&
                httpContext.Connection.RemoteIpAddress != null)
                ip = httpContext.Connection.RemoteIpAddress.ToString();

            if (ip.IsEmpty()) ip = GetHeaderValue(httpContext, "REMOTE_ADDR");

            return ip;
        }

        public static async Task<T?> DeserializeRequestJsonBodyAsAsync<T>(this HttpContext httpContext) where T : class
        {
            RequestSanityCheck(httpContext);
            var request = httpContext.Request;
            using var bodyReader = new StreamReader(request.Body, Encoding.UTF8);
            var body = await bodyReader.ReadToEndAsync();
            request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
            return body.Deserialize<T>();
        }

        /// <summary>
        /// Reads `request.Body` as string.
        /// </summary>
        public static async Task<string?> ReadRequestBodyAsStringAsync(this HttpContext httpContext)
        {
            RequestSanityCheck(httpContext);
            var request = httpContext.Request;
            using var bodyReader = new StreamReader(request.Body, Encoding.UTF8);
            var body = await bodyReader.ReadToEndAsync();
            request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
            return body;
        }

        /// <summary>
        /// Deserialize `request.Body` as a JSON content.
        /// </summary>
        public static async Task<Dictionary<string, string>?> DeserializeRequestJsonBodyAsDictionaryAsync(
            this HttpContext httpContext)
        {
            RequestSanityCheck(httpContext);
            var request = httpContext.Request;
            using var bodyReader = new StreamReader(request.Body, Encoding.UTF8);
            var body = await bodyReader.ReadToEndAsync();
            request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(body);
        }

        /// <summary>_</summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public static async Task<string> CurrentAccessToken(this HttpContext httpContext)
        {
            return httpContext.User.Identity != null && httpContext.User.Identity.IsAuthenticated
                ? await httpContext.GetTokenAsync("access_token") ?? string.Empty
                : string.Empty;
        }

        /// <summary>_</summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public static async Task<string> CurrentRefreshToken(this HttpContext httpContext)
        {
            return httpContext.User.Identity != null && httpContext.User.Identity.IsAuthenticated
                ? await httpContext.GetTokenAsync("refresh_token") ?? string.Empty
                : string.Empty;
        }

        #region Helpers

        private static void RequestSanityCheck(this HttpContext httpContext)
        {
            httpContext.NotNull(nameof(httpContext));

            if (httpContext.Request is null) throw new NullReferenceException("HttpContext.Request is null.");
        }

        private static IEnumerable<string> SplitCsv(string csvList, bool nullOrWhitespaceInputReturnsNull = false)
        {
            if (csvList.IsEmpty()) return (nullOrWhitespaceInputReturnsNull ? null : new List<string>()) ?? new List<string>();

            return csvList
                .TrimEnd(',')
                .Split(',')
                .AsEnumerable()
                .Select(s => s.Trim())
                .ToList();
        }

        #endregion
    }
}