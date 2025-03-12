#region

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Exceptions;
using Kharazmi.Functional;
using Kharazmi.Handlers;
using Kharazmi.Messages;
using Kharazmi.Validations;

#endregion

namespace Kharazmi.Pipelines
{
    [Pipeline("ValidationCommandPipeline", PipelineType.DomainCommand)]
    internal class ValidationCommandPipeline<TCommand> : CommandHandler<TCommand>, ICommandPipeline
        where TCommand : class,  ICommand
    {
        private readonly ICommandHandler<TCommand> _handler;
        private readonly IValidationHandler<TCommand> _validationHandler;

        public ValidationCommandPipeline(
            IServiceProvider sp,
            ICommandHandler<TCommand> handler,
            IValidationHandler<TCommand> validationHandler) : base(sp)
        {
            _handler = handler;
            _validationHandler = validationHandler;
        }


        protected override Task<Result> HandleAsync(CancellationToken token = default)
        {
            var failures = _validationHandler.Validate(Command).ToList();
            if (!failures.Any()) return _handler.HandleAsync(Command, DomainMetadata, token);

            PrintFailed(Command, DomainMetadata, failures);

            throw DomainException.For(Result.Fail("Command validation failed")
                .WithValidationMessages(failures));
        }
    }
}