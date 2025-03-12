using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Functional;

namespace Kharazmi.Bus
{
    public interface IBusMessageStore
    {
        Task<Result<bool>> IsSetAsync(string messageId, string? nameSpace = null, CancellationToken token = default);

        Task<IEnumerable<BusMessageStored>> GetAsync(int maxCount = 1000,
            CancellationToken token = default);

        Task<Result> TryAddOrUpdateAsync(string messageId, string? nameSpace = null, TimeSpan? expireAt = null, CancellationToken token = default);
        Task<Result> TryRemoveAsync(string messageId, string? nameSpace = null, CancellationToken token = default);
    }
}