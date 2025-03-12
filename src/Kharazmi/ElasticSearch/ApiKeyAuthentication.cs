using System;
using System.Security;
using Newtonsoft.Json;

namespace Kharazmi.ElasticSearch
{
    public class ApiKeyAuthentication
    {
        public ApiKeyAuthentication()
        {
        }

        public ApiKeyAuthentication(string id, string apiKey)
        {
            Id = id;
            ApiKey = apiKey;
        }

        public string Id { get; set; }
        public string ApiKey { get; set; }

        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore]

        public SecureString? SecureApiKey { get; set; }

        public ApiKeyAuthentication SetSecurePassword(Action<SecureString> setup)
        {
            SecureApiKey = new SecureString();
            setup.Invoke(SecureApiKey);
            return this;
        }
    }
}