using System;
using Kharazmi.Functional;

namespace Kharazmi.Pipelines
{
    public interface IPipelineLogProvider
    {
        (string messageTemplate, object[] args) BeforeHandleMessage(PipelineEventLog eventLog);
        (string messageTemplate, object[] args) OnHandleMessageFailed(PipelineEventLog eventLog, Result result);
        (string messageTemplate, object[] args) OnHandleMessageSuccess(PipelineEventLog eventLog, Result result);
        (string messageTemplate, object[] args) OnException(PipelineEventLog eventLog, Exception exception);
    }
}