using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel.Client;

namespace Kharazmi.Mvc.Extensions
{
    public static class HttpClientExtensions
    {
        /// <summary>_</summary>
        /// <param name="httpClient"></param>
        /// <returns></returns>
        public static Task<DiscoveryDocumentResponse> GetDiscoveryResponseAsync(this HttpClient httpClient)
        {
            
            return httpClient.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest
            {
                Policy = new DiscoveryPolicy
                {
                    RequireHttps = false
                }
            });
        }

        /// <summary>_</summary>
        /// <param name="httpClient"></param>
        /// <returns></returns>
        public static DiscoveryDocumentResponse GetDiscoveryResponse(this HttpClient httpClient)
        {
            var disco = httpClient.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest
            {
                Policy = new DiscoveryPolicy
                {
                    RequireHttps = false
                }
            }).ConfigureAwait(false).GetAwaiter().GetResult();

            return disco;
        }
    }
}