#region

using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http;
using System.Threading;
using Kharazmi.Extensions;
using Newtonsoft.Json;

#endregion

namespace Kharazmi.Http
{
    [JsonObject(MemberSerialization.OptIn)]
    public class HttpClientFactory :  IHttpClientFactory, IDisposable
    {
        private readonly ConcurrentDictionary<Uri, Lazy<HttpClient>> _httpClients = new();

        public static TimeSpan DefaultDnsRefreshTimeout = TimeSpan.FromMinutes(1);
        public static int DefaultConnectionLimit = 1024;
        public static int ConnectionLeaseTimeout = 60 * 1000; // 1 minute

        /// <summary>
        /// Reusing a single HttpClient instance across a multi-threaded application
        /// </summary>
        public HttpClientFactory()
        {
            ServicePointManager.DnsRefreshTimeout = (int) DefaultDnsRefreshTimeout.TotalMilliseconds;
            ServicePointManager.DefaultConnectionLimit = DefaultConnectionLimit;
        }

        public static HttpClientFactory Instance => new();

        /// <summary>
        /// Reusing a single HttpClient instance across a multi-threaded application
        /// </summary>
        public HttpClient GetOrCreate(string baseAddress, HttpMessageHandler? primaryHandler = null)
        {
            var baseUrl = new Uri(baseAddress);

            return _httpClients.GetOrAdd(baseUrl, uri => new Lazy<HttpClient>(() =>
                {
                    var client = primaryHandler is null
                        ? new HttpClient {BaseAddress = uri}
                        : new HttpClient(primaryHandler, false) {BaseAddress = uri};
                    return client.WitConnectionLeaseTimeout(ConnectionLeaseTimeout);
                },
                LazyThreadSafetyMode.ExecutionAndPublication)).Value;
        }


        public void TryRemove(HttpClient httpClient)
        {
            var url = httpClient.BaseAddress;
            if (url.IsNull()) return;
            _httpClients.TryRemove(url, out var client);
            if (client.IsNull()) return;
            client.Value.Dispose();
        }

        public void Dispose()
        {
            foreach (var httpClient in _httpClients.Values)
                httpClient.Value.Dispose();
        }
    }
}