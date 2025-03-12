using System;
using System.Security;
using Newtonsoft.Json;

namespace Kharazmi.ElasticSearch
{
    public class Proxy
    {
        public Proxy()
        {
        }

        public Proxy(string address, string username, string password)
        {
            Address = address;
            Username = username;
            Password = password;
        }

        public string Address { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
        public SecureString? SecurePassword { get; set; }

        public Proxy SetSecurePassword(Action<SecureString> setup)
        {
            SecurePassword = new SecureString();
            setup.Invoke(SecurePassword);
            return this;
        }
    }
}