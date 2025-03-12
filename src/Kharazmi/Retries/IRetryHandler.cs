#region

#endregion

using System;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Common.Metadata;
using Kharazmi.Functional;
using Kharazmi.Options;

namespace Kharazmi.Retries
{
    public interface IRetryHandler
    {
        Task<Result<TResult>> HandlerAsync<TResult>(IRetryOptions? retryOption, Func<Task<TResult>> invokeMethod,
            DomainMetadata context,
            CancellationToken token = default);
        
        Task<Result> HandlerAsync(IRetryOptions? retryOption, Func<Task> invokeMethod,
            DomainMetadata context,
            CancellationToken token = default);
    }
}