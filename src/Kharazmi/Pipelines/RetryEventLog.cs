using Kharazmi.Common.Metadata;
using Kharazmi.Constants;

namespace Kharazmi.Pipelines
{
    public record RetryEventLog
    {
        public RetryEventLog(
            DomainMetadata domainMetadata,
            string categoryName,
            int retryCounter = 0,
            double totalSeconds = 0,
            int attempt = 0,
            string? eventName = null)
        {
            EventName = eventName ?? MessageEventName.RetryHandleMessage;
            CategoryName = categoryName;
            DomainMetadata = domainMetadata;
            RetryCounter = retryCounter;
            TotalSeconds = totalSeconds;
            Attempt = attempt;
        }

        public string EventName { get; }
        public string CategoryName { get; }
        public int RetryCounter { get; set;}
        public double TotalSeconds { get; set; }
        public int Attempt { get; set;}
        public DomainMetadata DomainMetadata { get; }
    }
}