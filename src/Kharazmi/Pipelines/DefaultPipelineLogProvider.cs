using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Kharazmi.Constants;
using Kharazmi.Extensions;
using Kharazmi.Functional;
using Kharazmi.Runtime;

namespace Kharazmi.Pipelines
{
    internal class DefaultPipelineLogProvider : IPipelineLogProvider
    {
        private static readonly ConcurrentDictionary<int, IEnumerable<Property>> Properties = new();
        private static readonly TimeSpan Interval = TimeSpan.FromMinutes(15);
        private static readonly Timer Timer = new(Invalidate, null, Interval, TimeSpan.FromMilliseconds(-1));

        public DefaultPipelineLogProvider()
        {
        }

        public (string messageTemplate, object[] args) BeforeHandleMessage(PipelineEventLog eventLog)
        {
            var messageBuilder = MessageBuilder(eventLog);

            StringBuilder messageFormatBuilder = new();

            messageFormatBuilder.Append(MessageTemplate.BeforeMessageProcessing);
            messageFormatBuilder.Append(MetadataBuilder(eventLog, messageBuilder));
            messageFormatBuilder.Append(MessagePropertyBuilder(eventLog, messageBuilder));

            Timer.Change(Interval, TimeSpan.FromMilliseconds(-1));

            return (messageFormatBuilder.ToString(), messageBuilder.ToArray());
        }


        public (string messageTemplate, object[] args) OnHandleMessageFailed(PipelineEventLog eventLog, Result result)
        {
            var messageBuilder = MessageBuilder(eventLog);
            messageBuilder.Add(result.Description ?? "");

            StringBuilder messageFormatBuilder = new();

            messageFormatBuilder.Append(MessageTemplate.MessageProcessingFailed);

            Timer.Change(Interval, TimeSpan.FromMilliseconds(-1));

            return (messageFormatBuilder.ToString(), messageBuilder.ToArray());
        }

        public (string messageTemplate, object[] args) OnHandleMessageSuccess(PipelineEventLog eventLog, Result result)
        {
            var messageBuilder = MessageBuilder(eventLog);

            StringBuilder messageFormatBuilder = new();

            messageFormatBuilder.Append(MessageTemplate.MessageProcessSucceeded);

            Timer.Change(Interval, TimeSpan.FromMilliseconds(-1));

            return (messageFormatBuilder.ToString(), messageBuilder.ToArray());
        }

        public (string messageTemplate, object[] args) OnException(PipelineEventLog eventLog, Exception exception)
        {
            var messageBuilder = MessageBuilder(eventLog);
            messageBuilder.Add(exception.Message);

            StringBuilder messageFormatBuilder = new();

            messageFormatBuilder.Append(MessageTemplate.MessageProcessException);

            Timer.Change(Interval, TimeSpan.FromMilliseconds(-1));

            return (messageFormatBuilder.ToString(), messageBuilder.ToArray());
        }

        #region Helpers

        private static StringBuilder MetadataBuilder(PipelineEventLog @event, List<object> messageBuilder)
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

        private static StringBuilder MessagePropertyBuilder(PipelineEventLog @event, List<object> messageBuilder)
        {
            var props = Properties.GetOrAdd(@event.GetHashCode(),
                _ => @event.Message.GetAllPropertyValue(@event.MessageType));

            StringBuilder builder = new();
            builder.AppendFormat("{0}: ", MessageEventName.DomainMessage);

            foreach (var propValue in props)
            {
                builder.Append($"[{propValue.Name}: {{{propValue.Name}}}]");
                messageBuilder.Add(propValue.Value);
            }

            return builder;
        }

        private static List<object> MessageBuilder(PipelineEventLog eventLog)
        {
            return new()
            {
                eventLog.EventName,
                eventLog.CategoryName,
                eventLog.MessageType.GetGenericTypeName()
            };
        }

        private static void Invalidate(object? state)
        {
            Properties.Clear();
        }

        #endregion
    }
}