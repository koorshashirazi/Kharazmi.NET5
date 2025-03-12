using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Kharazmi.Options.SignalR
{
    public class HubConnectionOption : NestedOption
    {
        public HubConnectionOption()
        {
            CloseTimeout = TimeSpan.FromSeconds(5);
            Transports = HttpTransportType.Default;
            DefaultTransferFormat = TransferFormat.Binary;
            Headers = new Dictionary<string, string>();
        }

        public string? Url { get; set; }
        public bool SkipNegotiation { get; set; }
        public TimeSpan CloseTimeout { get; set; }
        public string Transports { get; set; }

        public IReadOnlyCollection<string> TransportTypes => HttpTransportType.Get();
        public string DefaultTransferFormat { get; set; }
        public IReadOnlyCollection<string> TransferFormats => TransferFormat.Get();
        public bool? UseDefaultCredentials { get; set; }
        public IDictionary<string, string> Headers { get; set; }

        [Newtonsoft.Json.JsonIgnore, JsonIgnore]
        public X509CertificateCollection? ClientCertificates { get; set; }

        [Newtonsoft.Json.JsonIgnore, JsonIgnore]
        public CookieContainer? Cookies { get; set; }

        [Newtonsoft.Json.JsonIgnore, JsonIgnore]
        public Func<Task<string>>? AccessTokenProvider { get; set; }

        [Newtonsoft.Json.JsonIgnore, JsonIgnore]
        public ICredentials? Credentials { get; set; }

        [Newtonsoft.Json.JsonIgnore, JsonIgnore]
        public IWebProxy? Proxy { get; set; }

        [Newtonsoft.Json.JsonIgnore, JsonIgnore]
        public Action<ClientWebSocketOptions>? WebSocketConfiguration { get; set; }

        [Newtonsoft.Json.JsonIgnore, JsonIgnore]
        public Func<HttpMessageHandler, HttpMessageHandler>? HttpMessageHandlerFactory { get; set; }

        public override void Validate()
        {
          
        }
    }
}