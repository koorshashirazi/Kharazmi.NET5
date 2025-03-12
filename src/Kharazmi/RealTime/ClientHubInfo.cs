using System;
using System.Text.Json.Serialization;
using Kharazmi.Common.Domain;
using Kharazmi.Common.Events;
using Kharazmi.Common.Metadata;
using Kharazmi.Common.ValueObjects;
using Kharazmi.Helpers;

namespace Kharazmi.RealTime
{
    public class ClientHubInfo : AggregateRootCache<string>
    {
        [Newtonsoft.Json.JsonConstructor, JsonConstructor]
        public ClientHubInfo(ConnectionId id) : base(id)
        {
            Metadata = new MetadataCollection();
            ConnectedAt = DateTimeHelper.DateTimeUtcNow;
        }

        public static ClientHubInfo Create(ConnectionId id) => new(id);

        public DateTime ConnectedAt { get; set; }
        public DateTime? DisconnectedAt { get; set; }

        public string? ClientUrl { get; set; }
        public DomainId? DomainId { get; set; }
        public UserId? UserId { get; set; }
        public string? ClientGroup { get; set; }
        public MetadataCollection Metadata { get; set; }

        public static implicit operator ClientHubInfo(string id)
            => Create(new ConnectionId(id));

        protected override void EnsureValidState()
        {
        }

        protected override void ApplyWhen(IAggregateEvent @event)
        {
        }

        public override IAggregateRootCache GetFromCache(object? value)
        {
            if (value is not ClientHubInfo clientHubInfo) return this;
            UpdateId(ConnectionId.From(clientHubInfo.Id));
            ConnectedAt = clientHubInfo.ConnectedAt;
            DisconnectedAt = clientHubInfo.DisconnectedAt;
            ClientUrl = clientHubInfo.ClientUrl;
            DomainId = clientHubInfo.DomainId;
            UserId = clientHubInfo.UserId;
            ClientGroup = clientHubInfo.ClientGroup;
            Metadata = clientHubInfo.Metadata;
            return this;
        }
    }
}