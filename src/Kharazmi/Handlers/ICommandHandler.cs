#region

using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Common.Metadata;
using Kharazmi.Functional;
using Kharazmi.Messages;

#endregion

namespace Kharazmi.Handlers
{
    public interface ICommandHandler<in TCommand>
        where TCommand : ICommand
    {
        Task<Result> HandleAsync(TCommand command, DomainMetadata domain,
            CancellationToken token = default);
    }

    public class NullCommandHandler : ICommandHandler<NullCommand>
    {
        public Task<Result> HandleAsync(NullCommand command, DomainMetadata domain,
            CancellationToken token = default)
            => Task.FromResult(Result.Ok());
    }

    public class NullCommand : Command
    {
        public NullCommand()
        {
        }
    }
}