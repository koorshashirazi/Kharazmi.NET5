#region

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kharazmi.Extensions;
using Kharazmi.Mvc.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

#endregion

namespace Kharazmi.Mvc.Http
{
    /// <summary>
    /// Http Request Info
    /// </summary>
    public class HttpRequestAccessor : IHttpRequestAccessor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUrlHelper? _urlHelper;

        public HttpRequestAccessor(IHttpContextAccessor httpContextAccessor, IUrlHelper urlHelper)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _urlHelper = urlHelper;
        }

        public string? GetUserAgent()
            => GetHeaderValue("User-Agent");

        public string? GetReferrerUrl()
            => _httpContextAccessor.HttpContext?.GetReferrerUrl();

        public Uri? GetReferrerUri()
            => _httpContextAccessor.HttpContext?.GetReferrerUri();

        public string? GetIp(bool tryUseXForwardHeader = true)
            => _httpContextAccessor.HttpContext.FindUserIp(tryUseXForwardHeader);

        public string? GetHeaderValue(string headerName)
            => _httpContextAccessor.HttpContext?.GetHeaderValue(headerName);

        public Uri? AbsoluteContent(string contentPath)
        {
            var baseUrl = GetBaseUri();
            return baseUrl is null ? null : new Uri(baseUrl, _urlHelper?.Content(contentPath));
        }

        public Uri? GetBaseUri()
        {
            var baseUrl = _httpContextAccessor.HttpContext?.GetBaseUrl();
            return !string.IsNullOrWhiteSpace(baseUrl) ? new Uri(baseUrl) : null;
        }

        public string? GetBaseUrl()
            => _httpContextAccessor.HttpContext?.GetBaseUrl();

        public string? GetRawUrl()
            => _httpContextAccessor.HttpContext?.GetRawUrl();

        public Uri? GetRawUri()
        {
            var rawUrl = _httpContextAccessor.HttpContext?.GetRawUrl();
            return !string.IsNullOrWhiteSpace(rawUrl) ? new Uri(rawUrl) : null;
        }

        public IUrlHelper? GetUrlHelper()
            => _urlHelper;

        public Task<T?> DeserializeRequestJsonBodyAsAsync<T>() where T : class
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext.IsNull()) return Task.FromResult<T>(default!)!;

            return httpContext.DeserializeRequestJsonBodyAsAsync<T>();
        }

        public Task<string?> ReadRequestBodyAsStringAsync()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext.IsNull()) return Task.FromResult<string>(default!)!;

            return httpContext.ReadRequestBodyAsStringAsync();
        }

        public Task<Dictionary<string, string>?> DeserializeRequestJsonBodyAsDictionaryAsync()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext.IsNull()) return Task.FromResult<Dictionary<string, string>>(default!)!;

            return httpContext.DeserializeRequestJsonBodyAsDictionaryAsync();
        }
    }
}