#region

#endregion

using System;
using System.Linq;
using System.Threading;
using Kharazmi.Common.Metadata;
using Newtonsoft.Json;

namespace Kharazmi.Common.Domain
{
    /// <summary>_</summary>
    [Serializable]
    public sealed class DomainContext
    {
        /// <summary>_ </summary>
        public string DomainId { get; }

        /// <summary>_ </summary>
        public string? DomainIdHeader { get; private set; }

        /// <summary>_ </summary>

        public string? RequestHeader { get; private set; }

        /// <summary>_ </summary>
        public string? MessageType { get; private set; }

        /// <summary>_ </summary>
        public string? MessageId { get; private set; }

        /// <summary>_ </summary>
        public string? UserId { get; private set; }

        /// <summary>_ </summary>
        public string? ResourceId { get; private set; }

        /// <summary>_ </summary>
        public string? Resource { get; private set; }

        /// <summary>_ </summary>
        public string? TraceId { get; private set; }

        /// <summary>_ </summary>
        public string? ConnectionId { get; private set; }

        /// <summary>_ </summary>
        public string? Origin { get; private set; }

        /// <summary>_ </summary>
        public string? RequestPath { get; private set; }

        /// <summary>_ </summary>
        public string Culture { get; } = Thread.CurrentThread.CurrentCulture.Name;

        /// <summary>_ </summary>
        public string CreatedAt { get; } = DateTimeConstants.UtcNow.ToString("g");

        /// <summary>_ </summary>
        public string UpdatedAt { get; private set; } = DateTimeConstants.UtcNow.ToString("g");

        /// <summary>_ </summary>
        public int Retries { get; private set; }


        /// <summary>_</summary>
        /// <param name="domainId"></param>
        /// <param name="requestHeader"></param>
        /// <param name="messageType"></param>
        /// <param name="messageId"></param>
        /// <param name="userId"></param>
        /// <param name="resourceId"></param>
        /// <param name="resource"></param>
        /// <param name="traceId"></param>
        /// <param name="connectionId"></param>
        /// <param name="origin"></param>
        /// <param name="requestPath"></param>
        /// <param name="retries"></param>
        /// <param name="domainIdHeader"></param>
        [JsonConstructor]
        public DomainContext(
            string domainId,
            string domainIdHeader,
            string requestHeader,
            string messageType,
            string messageId,
            string userId,
            string resourceId,
            string resource,
            string traceId,
            string connectionId,
            string origin,
            string requestPath,
            int retries = 0)
        {
            DomainId = domainId;
            DomainIdHeader = domainIdHeader;
            RequestHeader = requestHeader;
            MessageType = messageType;
            MessageId = messageId;
            UserId = userId;
            ResourceId = resourceId;
            Resource = resource;
            TraceId = traceId;
            ConnectionId = connectionId;
            Origin = origin;
            RequestPath = requestPath;
            Retries = retries;
        }

        /// <summary>_ </summary>
        public static DomainContext From<T>(DomainContext value) =>
            Create(value.DomainId)
                .WithMessageId(value.MessageId)
                .WithMessageType(typeof(T).Name)
                .WithOrigin(value.Origin)
                .WithResource(value.Resource)
                .WithRetry(value.Retries)
                .WithConnectionId(value.ConnectionId)
                .WithResourceId(value.ResourceId)
                .WithRequestPath(value.RequestPath)
                .WithTraceId(value.TraceId)
                .WithUserId(value.UserId);

        /// <summary>_ </summary>
        public static DomainContext Empty() => Create(null, "NOT_SET", MetadataKeys.DomainIdHeader);

        /// <summary>_ </summary>
        public static DomainContext Create(string domainId, string? domainIdHeader = null) =>
            Create(null, domainId, domainIdHeader ?? MetadataKeys.DomainIdHeader);

        /// <summary>_ </summary>
        public static DomainContext Create<T>(string domainId, string? domainIdHeader = null) =>
            Create(typeof(T).Name, domainId, domainIdHeader ?? MetadataKeys.DomainIdHeader);

        /// <summary>_ </summary>
        public static DomainContext Create(string? messageType, string domainId, string domainIdHeader)
        {
            if (messageType == null) throw new ArgumentNullException(nameof(messageType));

            return new DomainContext(domainId, domainIdHeader, "", messageType, "", "", "", "", "", ",", ",", "");
        }

        /// <summary>_ </summary>
        public DomainContext WithMessageId(string? value)
        {
            MessageId = value;
            return this;
        }

        /// <summary>_ </summary>
        public DomainContext WithDomainIdHeader(string? value)
        {
            DomainIdHeader = value;
            return this;
        }


        /// <summary>_ </summary>
        public DomainContext WithRequestHeader(string? value)
        {
            RequestHeader = value;
            return this;
        }

        /// <summary>_ </summary>
        public DomainContext WithMessageType(string? value)
        {
            MessageId = value;
            return this;
        }

        /// <summary>_ </summary>
        public DomainContext WithUserId(string? value)
        {
            UserId = value;
            return this;
        }

        /// <summary>_ </summary>
        public DomainContext WithResourceId(string? value)
        {
            ResourceId = value;
            return this;
        }

        /// <summary>_ </summary>
        public DomainContext WithResource(string? value)
        {
            Resource = value;
            return this;
        }

        /// <summary>_ </summary>
        public DomainContext WithTraceId(string? value)
        {
            TraceId = value;
            return this;
        }

        /// <summary>_ </summary>
        public DomainContext WithConnectionId(string? value)
        {
            ConnectionId = value;
            return this;
        }

        /// <summary>_ </summary>
        public DomainContext WithOrigin(string? value)
        {
            Origin = value;
            return this;
        }

        /// <summary>_ </summary>
        public DomainContext WithRequestPath(string? value)
        {
            RequestPath = value;
            return this;
        }

        /// <summary>_ </summary>
        public DomainContext WithRetry(int value)
        {
            Retries = value;
            return this;
        }

        /// <summary>_ </summary>
        public DomainContext UpdateAt(DateTime value)
        {
            var date = value.ToUtc();
            var isValid = DateTime.TryParse(CreatedAt, out var result);
            var createdAt = result.ToUtc();


            if (value == default) return this;
            if (!isValid || date < createdAt)
                throw new InvalidTimeZoneException("Updated dateTime can't be lower than created DateTime");

            UpdatedAt = createdAt.ToString("g");
            return this;
        }

        /// <summary>_ </summary>
        public DomainContext IncreaseRetrying()
        {
            Retries += 1;
            return this;
        }

        private static string GetName(string name)
        {
            return Underscore(name).ToLowerInvariant();
        }


        private static string Underscore(string value)
        {
            return string.Concat(value.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x : x.ToString()));
        }
    }
}