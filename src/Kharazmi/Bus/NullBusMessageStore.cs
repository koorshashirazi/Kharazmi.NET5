using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Dependency;
using Kharazmi.Functional;

namespace Kharazmi.Bus
{
    internal class NullBusMessageStore : IBusMessageStore, INullInstance, IMustBeInstance, IShouldBeSingleton
    {
        public Task<Result<bool>> IsSetAsync(string messageId, string? nameSpace = null,
            CancellationToken token = default)
            => Task.FromResult(Result.OkAs(false));

        public Task<IEnumerable<BusMessageStored>> GetAsync(int maxCount = 1000, CancellationToken token = default)
            => Task.FromResult(Enumerable.Empty<BusMessageStored>());

        public Task<Result> TryAddOrUpdateAsync(string messageId, string? nameSpace = null, TimeSpan? expireAt = null,
            CancellationToken token = default)
            => Task.FromResult(Result.Fail("IBusMessageStore is disabled"));

        public Task<Result> TryRemoveAsync(string messageId, string? nameSpace = null,
            CancellationToken token = default)
            => Task.FromResult(Result.Fail("IBusMessageStore is disabled"));
    }
}