#region

using System.Net;
using Kharazmi.Guard;
using Microsoft.AspNetCore.Http;

#endregion

namespace Kharazmi.Mvc.Extensions
{
    public static class HttpRequestExtensions
    {
        private const string RequestedWithHeader = "X-Requested-With";
        private const string XmlHttpRequest = "XMLHttpRequest";
        private const string Origin = "Origin";

        public static bool IsAjaxRequest(this HttpRequest request)
        {
            request.NotNull(nameof(request));

            return request.Headers[RequestedWithHeader] == XmlHttpRequest;
        }

        public static bool IsLocalRequest(this HttpRequest request)
        {
            request.NotNull(nameof(request));

            var connection = request.HttpContext.Connection;
            if (connection.RemoteIpAddress != null)
                return connection.LocalIpAddress != null
                    ? connection.RemoteIpAddress.Equals(connection.LocalIpAddress)
                    : IPAddress.IsLoopback(connection.RemoteIpAddress);

            return connection.RemoteIpAddress is null && connection.LocalIpAddress is null;
        }

        public static string GetOrigin(this HttpRequest request)
        {
            if (request.Headers.TryGetValue("Origin", out var origin)) return origin;

            return "";
        }
    }
}