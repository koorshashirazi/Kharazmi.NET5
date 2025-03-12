#region

using System;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Functional;
using Kharazmi.Guard;
using Kharazmi.Handlers;
using Kharazmi.Messages;
using Kharazmi.Options.Domain;

#endregion

namespace Kharazmi.Pipelines
{
    [Pipeline("TransientFaultCommandPipeline", PipelineType.DomainCommand)]
    internal class TransientFaultCommandPipeline<TCommand> : CommandHandler<TCommand>, ICommandPipeline
        where TCommand : class, ICommand
    {
        private readonly ICommandHandler<TCommand> _handler;

        public TransientFaultCommandPipeline(
            IServiceProvider sp,
            ICommandHandler<TCommand> handler) : base(sp)
        {
            _handler = handler.NotNull(nameof(handler));
        }


        protected override async Task<Result> HandleAsync(CancellationToken token = default)
        {
            var result = await RetryHandler.HandlerAsync(Settings.Get<DomainOption>().RetryOption, () =>
                _handler.HandleAsync(Command, DomainMetadata, token),DomainMetadata, token);

            return result.Value ?? Result.Fail("");
        }
    }
}