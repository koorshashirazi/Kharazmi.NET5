#region

using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

#endregion

namespace Kharazmi.Handlers
{
    public interface IValidationHandler<in TRequest, TResponse>
    {
        Task<TResponse> HandleAsync([NotNull]TRequest request,
            CancellationToken cancellationToken = default);
    }
}