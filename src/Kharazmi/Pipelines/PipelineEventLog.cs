using System;
using Kharazmi.Common.Metadata;
using Kharazmi.Constants;

namespace Kharazmi.Pipelines
{
    public record PipelineEventLog
    {
        public PipelineEventLog(
            object message, 
            Type messageType,
            DomainMetadata domainMetadata,
            string categoryName,
            string? eventName = null)
        {
            Message = message;
            MessageType = messageType;
            DomainMetadata = domainMetadata;
            CategoryName = categoryName;
            EventName = eventName ?? MessageEventName.MessageHandler;
        }

        public object Message { get; }
        public Type MessageType { get; }
        public DomainMetadata DomainMetadata { get; }
        public string EventName { get; }
        public string CategoryName { get; }
    }
}