using System.Net.Security;
using System.Security.Authentication;
using Kharazmi.Constants;
using Kharazmi.Options.RabbitMq;

namespace Kharazmi.Extensions
{
    public static class EnumExtensions
    {
        public static SslProtocols MapSslProtocols(this string value)
        {
            return value switch
            {
                SslProtocolType.None => SslProtocols.None,
                SslProtocolType.Tls => SslProtocols.Tls,
                SslProtocolType.Tls11 => SslProtocols.Tls11,
                SslProtocolType.Tls12 => SslProtocols.Tls12,
                SslProtocolType.Tls13 => SslProtocols.Tls13,
                _ => SslProtocols.None
            };
        }

        public static SslPolicyErrors MapSslPolicyErrors(this string value)
        {
            return value switch
            {
                SslPolicyErrorsType.None => SslPolicyErrors.None,
                SslPolicyErrorsType.RemoteCertificateChainErrors => SslPolicyErrors.RemoteCertificateChainErrors,
                SslPolicyErrorsType.RemoteCertificateNameMismatch => SslPolicyErrors.RemoteCertificateNameMismatch,
                SslPolicyErrorsType.RemoteCertificateNotAvailable => SslPolicyErrors.RemoteCertificateNotAvailable,
                _ => SslPolicyErrors.None
            };
        }
    }
}