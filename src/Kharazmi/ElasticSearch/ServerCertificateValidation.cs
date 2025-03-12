using System;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json;

namespace Kharazmi.ElasticSearch
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ServerCertificateValidation
    {
        public ServerCertificateValidation(Func<object, X509Certificate, X509Chain, SslPolicyErrors, bool> callback)
        {
            Callback = callback;
        }

        public Func<object, X509Certificate, X509Chain, SslPolicyErrors, bool> Callback { get; set; }
    }
}