using Newtonsoft.Json;

namespace Kharazmi.Redis.Jobs
{
    /// <summary>_</summary>
    public class DomainEventSerialized
    {
        /// <summary>_</summary>
        /// <param name="busOptionKey"></param>
        /// <param name="eventType"></param>
        /// <param name="channelName"></param>
        /// <param name="eventData"></param>
        /// <param name="metadata"></param>
        /// <param name="commandFlags"></param>
        [JsonConstructor]
        public DomainEventSerialized(string busOptionKey, string eventType,
            string channelName,
            string eventData,
            string metadata,
            int commandFlags)
        {
            BusOptionKey = busOptionKey;
            EventType = eventType;
            EventData = eventData;
            Metadata = metadata;
            ChannelName = channelName;
            CommandFlags = commandFlags;
        }

        /// <summary>_</summary>
        public string BusOptionKey { get; }

        /// <summary>_</summary>
        public string EventType { get; }

        /// <summary>_</summary>
        public string EventData { get; }

        /// <summary>_</summary>
        public string Metadata { get; }

        /// <summary>_</summary>
        public string ChannelName { get; }

        /// <summary>_</summary>
        public int CommandFlags { get; }
    }
}