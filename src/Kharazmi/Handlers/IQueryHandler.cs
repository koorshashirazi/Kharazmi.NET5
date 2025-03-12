#region

using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Common.Metadata;
using Kharazmi.Functional;
using Kharazmi.Messages;

#endregion

namespace Kharazmi.Handlers
{
    public interface IQueryHandler<in TQuery, TResult> where TQuery : IQuery<TResult>
    {
        Task<Result<TResult>> HandleAsync(TQuery query, DomainMetadata domain,
            CancellationToken cancellationToken = default);

        Task<Result<TResult>> HandleAsync(IQuery<TResult> query, DomainMetadata domain,
            CancellationToken token = default);
    }
}