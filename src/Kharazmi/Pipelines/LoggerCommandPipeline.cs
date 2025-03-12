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
    [Pipeline("LoggerCommandPipeline", PipelineType.DomainCommand)]
    internal class LoggerCommandPipeline<TCommand> : CommandHandler<TCommand>, ICommandPipeline
        where TCommand : class, ICommand
    {
        private readonly ILogger<LoggerCommandPipeline<TCommand>> _logger;
        private readonly IPipelineLogProvider _logProvider;
        private readonly ICommandHandler<TCommand> _handler;

        public LoggerCommandPipeline(
            IServiceProvider sp,
            ILogger<LoggerCommandPipeline<TCommand>> logger,
            IPipelineLogProvider logProvider,
            ICommandHandler<TCommand> handler) : base(sp)
        {
            _logger = logger;
            _logProvider = logProvider;
            _handler = handler;
        }

        protected override async Task<Result> HandleAsync(CancellationToken token = default)
        {
            var eventLog = new PipelineEventLog(Message,
                Message.GetType(), DomainMetadata, typeof(LoggerCommandPipeline<TCommand>).GetGenericTypeName());
            var logger = LoggerFactory.CreateLogger<LoggerCommandPipeline<TCommand>>();

            try
            {
                var (beforeTemplate, beforeArgs) = _logProvider.BeforeHandleMessage(eventLog);
                if (beforeTemplate.IsNotEmpty())
                    logger.LogTrace(beforeTemplate, beforeArgs);

                var result = await _handler.HandleAsync(Command, DomainMetadata, token);

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