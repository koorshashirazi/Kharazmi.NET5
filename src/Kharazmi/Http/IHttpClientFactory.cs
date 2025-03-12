using System.Net.Http;

namespace Kharazmi.Http
{
    /// <summary>_</summary>
    public interface IHttpClientFactory
    {
        /// <summary>_</summary>
        /// <returns></returns>
        HttpClient GetOrCreate(string baseAddress, HttpMessageHandler? handler = null);

        void TryRemove(HttpClient httpClient);
    }
}