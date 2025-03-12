using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Kharazmi.Constants;
using Kharazmi.Extensions;

namespace Kharazmi.Options.RabbitMq
{
    public static class SslPolicyErrorsType
    {
        public const string None = "None";
        public const string RemoteCertificateNotAvailable = "RemoteCertificateNotAvailable";
        public const string RemoteCertificateNameMismatch = "RemoteCertificateNameMismatch";
        public const string RemoteCertificateChainErrors = "RemoteCertificateChainErrors";
    }

    public class SslOption : NestedOption
    {
        private readonly HashSet<string> _sslPolicyErrorsTypes;
        private readonly HashSet<string> _sslProtocolTypes;
        public SslOption()
        {
            Version = SslProtocolType.Tls;
            AcceptablePolicyErrors = SslPolicyErrorsType.None;
            _sslPolicyErrorsTypes = new HashSet<string>
            {
                SslPolicyErrorsType.None,
                SslPolicyErrorsType.RemoteCertificateNotAvailable,
                SslPolicyErrorsType.RemoteCertificateNameMismatch,
                SslPolicyErrorsType.RemoteCertificateChainErrors,
            };
            _sslProtocolTypes = new HashSet<string>
            {
                SslProtocolType.None,
                SslProtocolType.Tls,
                SslProtocolType.Tls11,
                SslProtocolType.Tls12,
                SslProtocolType.Tls13
            };
        }

        /// <summary>_</summary>
        public string AcceptablePolicyErrors { get; set; }

        /// <summary>_</summary>
        [StringLength(100)]
        public string? CertPassphrase { get; set; }

        /// <summary>_</summary>
        [StringLength(200)]
        public string? CertPath { get; set; }

        /// <summary>Flag specifying if Ssl should indeed be used.</summary>
        public bool Enabled { get; set; }

        /// <summary>_</summary>
        [StringLength(100)]
        public string? ServerName { get; set; }

        /// <summary>Retrieve or set the Ssl protocol version.</summary>
        public string Version { get; set; }

        public IReadOnlyCollection<string> SslPolicyErrorsTypes => _sslPolicyErrorsTypes;
        public IReadOnlyCollection<string> SslProtocolTypes => _sslProtocolTypes;

        public override void Validate()
        {
            if (!Enabled) return;

            if (CertPath.IsEmpty())
                AddValidation(MessageHelper.NullOrEmpty(MessageEventName.OptionsValidation,
                    nameof(MessageNamingConventions), nameof(CertPath)));
        }
    }
}