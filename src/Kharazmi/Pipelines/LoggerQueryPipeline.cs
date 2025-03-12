#region

using System;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Extensions;
using Kharazmi.Functional;
using Kharazmi.Handlers;
using Kharazmi.Messages;
using Microsoft.Extensions.Logging;

#endregion

namespace Kharazmi.Pipelines
{
    [Pipeline("LoggerQueryPipeline", PipelineType.DomainQuery)]
    internal class LoggerQueryPipeline<TQuery, TResult> : QueryHandler<TQuery, TResult>, IQueryPipeline
        where TQuery : class, IQuery<TResult>
    {
        private readonly IPipelineLogProvider _logProvider;
        private readonly IQueryHandler<TQuery, TResult> _handler;

        public LoggerQueryPipeline(
            IServiceProvider sp,
            IQueryHandler<TQuery, TResult> handler, IPipelineLogProvider logProvider) : base(sp)
        {
            _handler = handler;
            _logProvider = logProvider;
        }

        protected override async Task<Result<TResult>> HandleAsync(CancellationToken token = default)
        {
            var eventLog = new PipelineEventLog(Message,
                Message.GetType(), DomainMetadata, typeof(LoggerQueryPipeline<TQuery, TResult>).GetGenericTypeName());
            
            var logger = LoggerFactory.CreateLogger<LoggerQueryPipeline<TQuery, TResult>>();
            
            try
            {
                var (beforeTemplate, beforeArgs) = _logProvider.BeforeHandleMessage(eventLog);
                if (beforeTemplate.IsNotEmpty())
                    logger.LogTrace(beforeTemplate, beforeArgs);

                var result = await _handler.HandleAsync(Query, DomainMetadata, token);

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