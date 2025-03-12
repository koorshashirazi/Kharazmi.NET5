#region

using System;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Common.Events;
using Kharazmi.Extensions;
using Kharazmi.Functional;
using Kharazmi.Handlers;
using Microsoft.Extensions.Logging;

#endregion

namespace Kharazmi.Pipelines
{
    [Pipeline("LoggerDomainEventPipeline", PipelineType.DomainEvent)]
    internal class LoggerEventPipeline<TEvent> : Handlers.EventHandler<TEvent>, IEventPipeline
        where TEvent : class, IDomainEvent
    {
        private readonly IPipelineLogProvider _logProvider;
        private readonly IEventHandler<TEvent> _handler;

        public LoggerEventPipeline(
            IServiceProvider sp,
            IEventHandler<TEvent> handler, IPipelineLogProvider logProvider) : base(sp)
        {
            _handler = handler;
            _logProvider = logProvider;
        }

        protected override async Task<Result> HandleAsync(CancellationToken token = default)
        {
            var eventLog = new PipelineEventLog(Message,
                Message.GetType(), DomainMetadata, typeof(LoggerEventPipeline<TEvent>).GetGenericTypeName());
            
            var logger = LoggerFactory.CreateLogger<LoggerEventPipeline<TEvent>>();
            
            try
            {
                var (beforeTemplate, beforeArgs) = _logProvider.BeforeHandleMessage(eventLog);
                if (beforeTemplate.IsNotEmpty())
                    logger.LogTrace(beforeTemplate, beforeArgs);

                var result = await _handler.HandleAsync(Event, DomainMetadata, token);

                if (result.Failed)
                {
                    var (failedTemplate, failedArgs) = _logProvider.OnHandleMessageFailed(eventLog, result);
                    if (failedTemplate.IsNotEmpty())
                        logger.LogError(failedTemplate, failedArgs);
                    return result;
                }

                var (succeededTemplate, succeededArgs) = _logProvider.OnHandleMessageSuccess(eventLog, result);
                if (succeededTemplate.IsNotEmpty())
                    logger.LogInformation(succeededTemplate, succeededArgs);

                return result;
            }
            catch (Exception e)
            {
                var (exceptionTemplate, exceptionArgs) = _logProvider.OnException(eventLog, e);
                if (exceptionTemplate.IsNotEmpty())
                    logger.LogError(exceptionTemplate, exceptionArgs);

                throw new Exception(e.Message, e);
            }
        }
    }
}