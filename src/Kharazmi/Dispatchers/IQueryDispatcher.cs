#region

using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Common.Metadata;
using Kharazmi.Dependency;
using Kharazmi.Messages;

#endregion

namespace Kharazmi.Dispatchers
{
    public interface IQueryDispatcher: IMustBeInstance
    {
        Task<TResult?> QueryAsync<TResult>(
            [NotNull] IQuery<TResult> query,
            MetadataCollection? metadata = null,
            CancellationToken token = default);
    }
}