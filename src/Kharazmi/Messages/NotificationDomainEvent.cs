#region

using System.Diagnostics.CodeAnalysis;
using Kharazmi.Common.Events;
using Kharazmi.Common.ValueObjects;
using Kharazmi.Extensions;
using Kharazmi.Functional;

#endregion

namespace Kharazmi.Messages
{
    public class NotificationDomainEvent : DomainEvent
    {
        public NotificationDomainEvent()
        {
        }

        private NotificationDomainEvent(string? reason)
        {
            Reason = reason;
        }

        public string? ActionType { get; private set; }
        public string? Code { get; private set; }
        public string? Reason { get; private set; }
        public Result? Result { get; private set; }

        public static NotificationDomainEvent For(IIdentity aggregateId, string reason, string? code = default)
        {
            return (NotificationDomainEvent) new NotificationDomainEvent(reason).WithErrorCode(code)
                .SetAggregateId(aggregateId);
        }


        public static NotificationDomainEvent For(string? reason, string? code = default)
        {
            return new NotificationDomainEvent(reason).WithErrorCode(code);
        }


        public static NotificationDomainEvent From([NotNull] Result result)
        {
            return new NotificationDomainEvent().WithResult(result)
                .WithErrorCode(result.Code)
                .WithActionType(result.MessageType);
        }

        public NotificationDomainEvent WithResult([NotNull] Result value)
        {
            Result = value;
            return this;
        }

        public NotificationDomainEvent WithErrorCode(string? value)
        {
            Code = value;
            return this;
        }

        public NotificationDomainEvent WithReason(string? value)
        {
            Reason = value;
            return this;
        }

        public NotificationDomainEvent WithActionType(string? value)
        {
            if (value?.IsNotEmpty() == true)
                ActionType = value;
            return this;
        }
    }
}