#region

using Kharazmi.Exceptions;
using Kharazmi.Extensions;

#endregion

namespace Kharazmi.Http
{
    public class DiscoveryTokenOptions
    {
        public DiscoveryTokenOptions(string? authorityUrl, string? clientId, string? clientSecret)
        {
            if (authorityUrl.IsEmpty())
                throw new ArgumentEmptyException(GetType(), nameof(authorityUrl));

            if (clientId.IsEmpty())
                throw new ArgumentEmptyException(GetType(), nameof(clientId));

            if (clientSecret.IsEmpty())
                throw new ArgumentEmptyException(GetType(), nameof(clientSecret));

            AuthorityUrl = authorityUrl;
            ClientId = clientId;
            ClientSecret = clientSecret;
        }

        public static DiscoveryTokenOptions For(string? authorityUrl, string? clientId, string? clientSecret)
        {
            return new DiscoveryTokenOptions(authorityUrl, clientId, clientSecret);
        }

        public string? AuthorityUrl { get; }
        public string? ClientId { get; }
        public string? ClientSecret { get; }
    }
}