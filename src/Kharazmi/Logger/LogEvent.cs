using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Kharazmi.Logger
{
    [JsonObject(MemberSerialization.OptIn)]
    public class LogEvent
    {
        public LogEvent(DateTimeOffset timestamp, string levels, Exception exception, string messageTemplate,
            IReadOnlyDictionary<string, object> logEventProperties)
        {
            Timestamp = timestamp;
            Levels = levels;
            Exception = exception;
            MessageTemplate = messageTemplate;
            LogEventProperties = logEventProperties;
        }

        [JsonProperty] public DateTimeOffset Timestamp { get; }
        [JsonProperty] public string Levels { get; }
        public Exception Exception { get; }

        [JsonProperty] public string MessageTemplate { get; }
        public IReadOnlyDictionary<string, object> LogEventProperties { get; }
    }
}