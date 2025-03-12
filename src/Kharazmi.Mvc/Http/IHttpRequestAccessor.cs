#region

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kharazmi.Dependency;
using Microsoft.AspNetCore.Mvc;

#endregion

namespace Kharazmi.Mvc.Http
{
    /// <summary>
    /// HttpRequest Info
    /// </summary>
    public interface IHttpRequestAccessor : IShouldBeSingleton, IMustBeInstance
    {
        string? GetIp(bool tryUseXForwardHeader = true);
        string? GetHeaderValue(string headerName);
        string? GetUserAgent();
        string? GetReferrerUrl();
        Uri? GetReferrerUri();
        Uri? AbsoluteContent(string contentPath);
        Uri? GetBaseUri();
        string? GetBaseUrl();
        string? GetRawUrl();
        Uri? GetRawUri();
        IUrlHelper? GetUrlHelper();
        Task<T?>? DeserializeRequestJsonBodyAsAsync<T>() where T : class;
        Task<string?>? ReadRequestBodyAsStringAsync();
        Task<Dictionary<string, string>?>? DeserializeRequestJsonBodyAsDictionaryAsync();
    }
}