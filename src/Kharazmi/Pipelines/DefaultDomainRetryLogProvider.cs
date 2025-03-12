using System;
using System.Collections.Generic;
using System.Text;
using Kharazmi.Constants;

namespace Kharazmi.Pipelines
{
    internal class DefaultDomainRetryLogProvider : IDomainRetryLogProvider
    {
        public DefaultDomainRetryLogProvider()
        {
        }

        public (string messageTemplate, object[] args) BeforeHandleMessage(RetryEventLog eventLog)
        {
            var messageBuilder = MessageBuilder(eventLog);
            messageBuilder.Add(eventLog.DomainMetadata.DomainId);

            StringBuilder messageFormatBuilder = new();

            messageFormatBuilder.Append(MessageTemplate.DomainRetryBeforeHandleMessage);
            messageFormatBuilder.Append(MetadataBuilder(eventLog, messageBuilder));

            return (messageFormatBuilder.ToString(), messageBuilder.ToArray());
        }

        public (string messageTemplate, object[] args) AfterHandleMessage(RetryEventLog eventLog)
        {
            var messageBuilder = MessageBuilder(eventLog);
            messageBuilder.Add(eventLog.DomainMetadata.DomainId);
            messageBuilder.Add(eventLog.TotalSeconds);

            StringBuilder messageFormatBuilder = new();

            messageFormatBuilder.Append(MessageTemplate.DomainRetryAfterHandleMessage);

            return (messageFormatBuilder.ToString(), messageBuilder.ToArray());
        }

        public (string messageTemplate, object[] args) OnException(RetryEventLog eventLog, Exception exception)
        {
            var messageBuilder = MessageBuilder(eventLog);
            messageBuilder.Add(eventLog.DomainMetadata.DomainId);
            messageBuilder.Add(exception.Message);

            StringBuilder messageFormatBuilder = new();

            messageFormatBuilder.Append(MessageTemplate.DomainRetryException);

            return (messageFormatBuilder.ToString(), messageBuilder.ToArray());
        }

        public (string messageTemplate, object[] args) OnRetry(RetryEventLog eventLog, Exception exception)
        {
            var messageBuilder = MessageBuilder(eventLog);
            messageBuilder.Add(eventLog.DomainMetadata.DomainId);
            messageBuilder.Add(exception.GetType().Name);
            messageBuilder.Add(exception.Message);
            messageBuilder.Add(eventLog.Attempt);

            StringBuilder messageFormatBuilder = new();

            messageFormatBuilder.Append(MessageTemplate.DomainRetry);

            return (messageFormatBuilder.ToString(), messageBuilder.ToArray());
        }

        #region Helpers

        private static StringBuilder MetadataBuilder(RetryEventLog @event, List<object> messageBuilder)
        {
            StringBuilder builder = new();
            builder.AppendFormat(", {0}: ", MessageEventName.DomainMetadata);

            foreach (var (key, value) in @event.DomainMetadata)
            {
                builder.Append($"[{key}: {{{key}}}]");
                messageBuilder.Add(value);
            }

            return builder;
        }

        private static List<object> MessageBuilder(RetryEventLog eventLog)
        {
            return new()
            {
                eventLog.EventName,
                eventLog.CategoryName
            };
        }

        #endregion
    }
}