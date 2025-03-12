using System;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Common.Metadata;
using Kharazmi.Dependency;
using Kharazmi.Functional;
using Kharazmi.Messages;

namespace Kharazmi.Dispatchers
{
    public class NullDomainDispatcher : IDomainDispatcher, INullInstance
    {
        public Task<Result> RaiseAsync(object? domainEvent,Type eventType, MetadataCollection? metadata = null, CancellationToken token = default)
            => Task.FromResult(Result.Fail("Can't resolve implementation service of IDomainDispatcher"));
     
        public Task<Result> SendAsync(object? command,Type commandType, MetadataCollection? metadata = null,
            CancellationToken token = default)
            => Task.FromResult(Result.Fail("Can't resolve implementation service of IDomainDispatcher"));

        public Task<TResult?> QueryAsync<TResult>(IQuery<TResult> query, MetadataCollection? metadata = null,
            CancellationToken token = default)
            => Task.FromResult<TResult>(default!)!;

        public Task<Result> SendAsync(object? command, Type commandType, bool dispatchAsynchronous = false,
            MetadataCollection? metadata = null, CancellationToken token = default)
            => Task.FromResult(Result.Fail("Can't resolve implementation service of IDomainDispatcher"));

        public Task<Result> RaiseAsync(object? domainEvent, Type eventType, bool dispatchAsynchronous = false,
            MetadataCollection? metadata = null, CancellationToken token = default)
            => Task.FromResult(Result.Fail("Can't resolve implementation service of IDomainDispatcher"));
    }
}