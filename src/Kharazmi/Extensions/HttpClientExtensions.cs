using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Kharazmi.Guard;

namespace Kharazmi.Extensions
{
    public static class HttpClientExtensions
    {
        public static void SetBasicAuthentication(this HttpClient client,
            string username, string password)
        {
            var byteArray = System.Text.Encoding.ASCII.GetBytes($"{username}:{password}");
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Basic",
                    System.Convert.ToBase64String(byteArray));
        }
        
        /// <summary>_</summary>
        /// <param name="client"></param>
        /// <param name="value">ensures connections are not used indefinitely.</param>
        public static HttpClient WitConnectionLeaseTimeout(this HttpClient client, int value)
        {
            var baseUrl = client.BaseAddress.NotNull(nameof(client.BaseAddress));
            // keeps the connection open -> more efficient use of the client
            client.DefaultRequestHeaders.ConnectionClose = false;
            ServicePointManager.FindServicePoint(baseUrl).ConnectionLeaseTimeout = value;
            return client;
        }

        public static HttpClient WithRequestTimeout(this HttpClient client, TimeSpan value)
        {
            client.Timeout = value;
            return client;
        }

        public static HttpClient WithDefaultHeaders(this HttpClient client,
            IDictionary<string, string> defaultRequestHeaders)
        {
            foreach (var (key, value) in defaultRequestHeaders)
                client.DefaultRequestHeaders.Add(key, value);
            return client;
        }

        public static HttpClient WithMaxResponseBufferSize(this HttpClient client, long value)
        {
            client.MaxResponseContentBufferSize = value;
            return client;
        }
    }
}