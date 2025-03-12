#region

#endregion

using System;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Common.Metadata;
using Kharazmi.Functional;

namespace Kharazmi.Dispatchers
{
    public interface IDomainDispatcher : IEventDispatcher, ICommandDispatcher, IQueryDispatcher
    {
        Task<Result> SendAsync(
            object? command,
            Type commandType,
            bool dispatchAsynchronous,
            MetadataCollection? metadata = null,
          
            CancellationToken token = default);
        Task<Result> RaiseAsync(
            object? domainEvent,
            Type eventType,
            bool dispatchAsynchronous,
            MetadataCollection? metadata = null,
            CancellationToken token = default);
    }
}