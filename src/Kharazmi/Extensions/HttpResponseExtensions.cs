#region

using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

#endregion

namespace Kharazmi.Extensions
{
    public static class HttpResponseExtensions
    {
        public static async Task<T?> SerializeAsAsync<T>([NotNull] this HttpResponseMessage httpResponseMessage)
            where T : class
        {
            if (!httpResponseMessage.IsSuccessStatusCode) return default;

            await using var responseStream = await httpResponseMessage.Content.ReadAsStreamAsync();
            using var streamReader = new StreamReader(responseStream);
            using var jsonTextReader = new JsonTextReader(streamReader);
            return JsonSerializer.CreateDefault().Deserialize<T>(jsonTextReader);
        }
    }
}