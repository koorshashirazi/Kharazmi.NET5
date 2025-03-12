using System;
using System.Collections.Specialized;
using Kharazmi.Net;
using Kharazmi.Options;

namespace Kharazmi.ElasticSearch
{
    public class ConnectionConfiguration : NestedOption
    {
        private ServerCertificateValidation? _serverCertificateValidation;

        public ConnectionConfiguration()
        {
            SniffOnConnectionFault = true;
            SniffOnStartup = true;
            EnableHttpCompression = true;
            DisableAutomaticProxyDetection = true;
            ThrowExceptions = true;
            DisablePing = true;
            PrettyJson = true;
            IncludeServerStackTraceOnError = true;
            DisableDirectStreaming = true;
            EnableHttpPipelining = true;
            TransferEncodingChunked = true;

            SniffLifeSpan = TimeSpan.FromHours(1);
            RequestTimeout = TimeSpan.FromMinutes(1);
            PingTimeout = TimeSpan.FromSeconds(3);
            ConnectionLimit = NetHelper.CanUsingCurlHandler() ? Environment.ProcessorCount : 80;
        }

        public bool SniffOnConnectionFault { get; set; }
        public bool SniffOnStartup { get; set; }
        public bool EnableHttpCompression { get; set; }
        public bool EnableHttpPipelining { get; set; }
        public bool DisableAutomaticProxyDetection { get; set; }
        public bool ThrowExceptions { get; set; }
        public bool DisablePing { get; set; }
        public bool PrettyJson { get; set; }
        public bool IncludeServerStackTraceOnError { get; set; }
        public bool DisableDirectStreaming { get; set; }
        public bool TransferEncodingChunked { get; set; }
        public bool UseTcpKeepAlive { get; set; }
        public bool UseBasicAuthentication { get; set; }
        public bool UseApiKeyAuthentication { get; set; }
        public bool UseClientCertificates { get; set; }
        public bool UseProxy { get; set; }
        public int? MaximumRetries { get; set; }
        public int ConnectionLimit { get; set; }
        public int[]? SkipDeserializationForStatusCodes { get; set; }
        public string? UserAgent { get; set; }
        public TimeSpan SniffLifeSpan { get; set; }
        public TimeSpan RequestTimeout { get; set; }
        public TimeSpan PingTimeout { get; set; }
        public TimeSpan? DeadTimeout { get; set; }
        public TimeSpan? MaxDeadTimeout { get; set; }
        public TimeSpan? MaxRetryTimeout { get; set; }
        public TimeSpan? DnsRefreshTimeout { get; set; }
        public NameValueCollection? GlobalQueryStringParameters { get; set; }
        public NameValueCollection? GlobalHeaders { get; set; }
        public Proxy? Proxy { get; set; }
        public TcpKeepAlive? TcpKeepAlive { get; set; }
        public BasicAuthentication? BasicAuthentication { get; set; }
        public ApiKeyAuthentication? ApiKeyAuthentication { get; set; }
        public ClientCertificates? ClientCertificates { get; set; }

        public ServerCertificateValidation? ServerCertificateValidation() => _serverCertificateValidation;

        public ConnectionConfiguration SetServerCertificateValidation(ServerCertificateValidation value)
        {
            _serverCertificateValidation = value;
            return this;
        }

        public override void Validate()
        {
            if (UseTcpKeepAlive)
                TcpKeepAlive ??= new TcpKeepAlive();

            if (UseProxy)
                Proxy ??= new Proxy();

            if (UseBasicAuthentication)
                BasicAuthentication ??= new BasicAuthentication();

            if (UseApiKeyAuthentication)
                ApiKeyAuthentication ??= new ApiKeyAuthentication();

            if (UseClientCertificates)
                ClientCertificates ??= new ClientCertificates();
        }
    }
}