using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Collections;
using Kharazmi.Dependency;
using Kharazmi.Functional;
using Microsoft.Extensions.Caching.Distributed;

namespace Kharazmi.Caching
{
    public class NullDistributedCache : IDistributedCacheExtended, INullInstance, IShouldBeSingleton, IMustBeInstance
    {
        public byte[] Get(string key)
            => System.Text.Encoding.UTF8.GetBytes(key);

        public Task<byte[]> GetAsync(string key, CancellationToken token = default)
            => Task.FromResult(System.Text.Encoding.UTF8.GetBytes(key));

        public void Refresh(string key)
        {
        }

        public Task RefreshAsync(string key, CancellationToken token = default)
            => Task.CompletedTask;

        public void Remove(string key)
        {
        }

        public Task RemoveAsync(string key, CancellationToken token = default)
            => Task.CompletedTask;

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
        }

        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options,
            CancellationToken token = default) => Task.CompletedTask;

        public IEnumerable<string> GetKeys(string pattern = "*", int total = 1000)
            => Enumerable.Empty<string>();

        public Task<IEnumerable<string>> GetKeysAsync(string pattern = "*", int total = 1000,
            CancellationToken token = default)
            => Task.FromResult(Enumerable.Empty<string>());

        public Task<IPagedList<T>> GetAsync<T>(IEnumerable<string> keys, int pageNumber = 0, int pageSize = 100,
            int total = 1000, CancellationToken token = default) where T : class
            => Task.FromResult(PagedList<T>.Empty);

        public Task<Result> RemoveAsync(IEnumerable<string> keys, CancellationToken token = default)
            => Task.FromResult(Result.Fail("DistributedCache is not enabled"));

        public Task<Result> RemoveAllAsync(CancellationToken token = default)
            => Task.FromResult(Result.Fail("DistributedCache is not enabled"));
    }
}