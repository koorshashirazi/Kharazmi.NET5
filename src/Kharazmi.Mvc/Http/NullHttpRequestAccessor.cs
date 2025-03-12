using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kharazmi.Dependency;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Kharazmi.Mvc.Http
{
    internal class NullHttpRequestAccessor : IHttpRequestAccessor, INullInstance
    {
        public string GetIp(bool tryUseXForwardHeader = true)
            => "";

        public string GetHeaderValue(string headerName)
            => "";

        public string GetUserAgent()
            => "";

        public string GetReferrerUrl()
            => "";

        public Uri GetReferrerUri()
            => new Uri("");

        public Uri AbsoluteContent(string contentPath)
            => new Uri("");

        public Uri GetBaseUri()
            => new Uri("");

        public string GetBaseUrl()
            => "";

        public string GetRawUrl()
            => "";

        public Uri GetRawUri()
            => new Uri("");

        public IUrlHelper GetUrlHelper()
            => new UrlHelper(new ActionContext());

        public Task<T?> DeserializeRequestJsonBodyAsAsync<T>() where T : class
            => Task.FromResult<T>(default!)!;

        public Task<string?> ReadRequestBodyAsStringAsync()
            => Task.FromResult<string>(default!)!;

        public Task<Dictionary<string, string>?> DeserializeRequestJsonBodyAsDictionaryAsync()
            => Task.FromResult<Dictionary<string, string>>(default!)!;
    }
}