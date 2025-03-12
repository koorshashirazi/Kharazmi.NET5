namespace Kharazmi.Jobs
{
    public class MessageSerialized
    {
        public string? MessageType { get; }
        public string? MessageData { get; }
        public string? DomainContextMetadata { get; }

        public MessageSerialized(
            string? messageType, 
            string messageData, 
            string? domainContextMetadata = null)
        {
            MessageType = messageType;
            MessageData = messageData;
            DomainContextMetadata = domainContextMetadata;
        }
    }
}