#region

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Common.Metadata;
using Kharazmi.Extensions;
using Kharazmi.Functional;
using Kharazmi.Messages;

#endregion

namespace Kharazmi.Handlers
{
    public abstract class CommandHandler<TCommand, TResult> : MessageHandler<TCommand>, ICommandHandler<TCommand>
        where TCommand : class, ICommand
        where TResult : Result
    {
        protected CommandHandler(IServiceProvider sp) : base(sp)
        {
        }

        public abstract Task<Result> HandleAsync(TCommand command, DomainMetadata domain,
            CancellationToken token = default);

        public Task<Result> HandleAsync(ICommand command, DomainMetadata domain,
            CancellationToken token = default) => HandleAsync((TCommand) command, domain, token);
    }

    public abstract class CommandHandler<TCommand> : CommandHandler<TCommand, Result> where TCommand : class, ICommand
    {
        private TCommand? _command;


        protected TCommand Command
        {
            get => _command ?? throw new NullReferenceException(typeof(TCommand).Name);
            private set => _command = value;
        }


        protected CommandHandler(IServiceProvider sp) : base(sp)
        {
        }

        public override Task<Result> HandleAsync([NotNull] TCommand command, DomainMetadata domain,
            CancellationToken token = default)
        {
            try
            {
                token.ThrowIfCancellationRequested();
                Message = command;
                _command = command;
                MessageName = command.GetGenericTypeName();
                Stopwatch = System.Diagnostics.Stopwatch.StartNew();
                DomainMetadata = domain;
                return HandleAsync(token);
            }
            catch (Exception e)
            {
                OnExecuteFailed();
                e.AsDomainException();
                return Task.FromResult(Result.Fail(""));
            }
        }

        protected void IncreaseDomainRetry()
        {
            base.IncreaseDomainRetry(DomainMetadata);
        }

        protected abstract Task<Result> HandleAsync(CancellationToken token = default);
       
    }
}