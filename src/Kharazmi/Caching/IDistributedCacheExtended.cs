using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Collections;
using Kharazmi.Functional;
using Microsoft.Extensions.Caching.Distributed;

namespace Kharazmi.Caching
{
    public interface IDistributedCacheExtended : IDistributedCache
    {
        IEnumerable<string> GetKeys(string pattern = "*", int total = 1000);

        Task<IEnumerable<string>> GetKeysAsync(string pattern = "*", int total = 1000,
            CancellationToken token = default);

        Task<IPagedList<T>> GetAsync<T>(IEnumerable<string> keys, int pageNumber = 0, int pageSize = 100, int total = 1000,
            CancellationToken token = default) where T : class;
        Task<Result> RemoveAsync(IEnumerable<string> keys, CancellationToken token = default);
        Task<Result> RemoveAllAsync(CancellationToken token = default);
    }
}