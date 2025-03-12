#region

using Newtonsoft.Json;

#endregion

namespace Kharazmi.Common.Events
{
    /// <summary></summary>
    public interface IRejectedDomainEvent : IDomainEvent
    {
        /// <summary></summary>
        [JsonProperty]
        public object? DomainContext { get; set; }

        /// <summary></summary>
        [JsonProperty]
        string? Reason { get; set; }

        /// <summary>_</summary>
        /// <param name="domainEvent"></param>
        /// <returns></returns>
        IRejectedDomainEvent FromDomainEvent(object? domainEvent);
    }

    /// <summary></summary>
    public class RejectedDomainEvent : DomainEvent, IRejectedDomainEvent
    {
        /// <summary></summary>
        public RejectedDomainEvent()
        {
        }

        /// <summary> </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        public static IRejectedDomainEvent Create(string reason)
            => new RejectedDomainEvent().UpdateReason(reason);

        /// <summary></summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public IRejectedDomainEvent UpdateReason(string value)
        {
            Reason = value;
            return this;
        }

        /// <summary>_</summary>
        /// <param name="domainEvent"></param>
        /// <returns></returns>
        public IRejectedDomainEvent FromDomainEvent(object? domainEvent)
        {
            if (domainEvent is null) return this;
            DomainEvent = domainEvent;
            return this;
        }

        [JsonProperty]  public object? DomainContext { get; set; }
        [JsonProperty] public string? Reason { get; set; }
        [JsonProperty] public object? DomainEvent { get; set; }
    }
}