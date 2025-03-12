using System;
using System.Security;
using Newtonsoft.Json;

namespace Kharazmi.ElasticSearch
{
    public class BasicAuthentication
    {
        public BasicAuthentication()
        {
        }

        public BasicAuthentication(string username, string password)
        {
            Username = username;
            Password = password;
        }

        public string Username { get; set; }
        public string Password { get; set; }

        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore]

        public SecureString? SecurePassword { get; set; }

        public BasicAuthentication SetSecurePassword(Action<SecureString> setup)
        {
            SecurePassword = new SecureString();
            setup.Invoke(SecurePassword);
            return this;
        }
    }
}