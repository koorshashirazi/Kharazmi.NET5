#region

using System;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Common.Metadata;
using Kharazmi.Dependency;
using Kharazmi.Functional;

#endregion

namespace Kharazmi.Dispatchers
{
    public interface IEventDispatcher : IMustBeInstance
    {
        Task<Result> RaiseAsync(
            object? domainEvent,
            Type eventTypes,
            MetadataCollection? metadata = null,
            CancellationToken token = default);
    }
}