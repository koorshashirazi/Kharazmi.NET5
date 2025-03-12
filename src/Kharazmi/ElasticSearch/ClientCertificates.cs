using System.Data;
using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json;

namespace Kharazmi.ElasticSearch
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ClientCertificates
    {
        public ClientCertificates()
        {
        }

        public ClientCertificates(
            string? certificatePath = null,
            X509Certificate? certificate = null,
            X509CertificateCollection? certificates = null)
        {
            if (certificates is null && certificate is null && certificatePath is null)
                throw new NoNullAllowedException(
                    "Client certificates can't accept null value for both certificates and certificate");

            CertificatePath = certificatePath;
            Certificates = certificates;
            Certificate = certificate;
        }

        [JsonProperty] public string? CertificatePath { get; set; }
        public X509CertificateCollection? Certificates { get; set; }
        public X509Certificate? Certificate { get; set; }
    }
}