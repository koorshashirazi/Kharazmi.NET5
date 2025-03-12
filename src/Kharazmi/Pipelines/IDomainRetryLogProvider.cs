using System;

namespace Kharazmi.Pipelines
{
    public interface IDomainRetryLogProvider
    {
        (string messageTemplate, object[] args) BeforeHandleMessage(RetryEventLog eventLog);
        (string messageTemplate, object[] args) AfterHandleMessage(RetryEventLog eventLog);
        (string messageTemplate, object[] args) OnException(RetryEventLog eventLog, Exception exception);
        (string messageTemplate, object[] args) OnRetry(RetryEventLog eventLog, Exception exception);
    }
}