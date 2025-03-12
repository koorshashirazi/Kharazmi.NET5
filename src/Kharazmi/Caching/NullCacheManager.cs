using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Collections;
using Kharazmi.Common.Caching;
using Kharazmi.Common.ValueObjects;
using Kharazmi.Dependency;
using Kharazmi.Functional;

namespace Kharazmi.Caching
{
    public class NullCacheManager : ICacheManager, INullInstance
    {
        public void Dispose()
        {
        }

        public Result<bool> Exist(string key) => Result.FailAs<bool>("");

        public Task<Result<bool>> ExistAsync(string key, CancellationToken token = default)
            => Task.FromResult(Result.FailAs<bool>(""));

        public Result<bool> Exist<T>(T value) where T : class, ICacheItem
            => Result.FailAs<bool>("");

        public Task<Result<bool>> ExistAsync<T>(T value, CancellationToken token = default) where T : class, ICacheItem
            => Task.FromResult(Result.FailAs<bool>(""));

        public Maybe<object> Find(string key)
            => Maybe<object>.None;

        public Task<Maybe<object>> FindAsync(string key, CancellationToken token = default)
            => Task.FromResult(Maybe<object>.None);

        public Maybe<T> Find<T>(string key) where T : class, ICacheItem
            => Maybe<T>.None;

        public Task<Maybe<T>> FindAsync<T>(string key, CancellationToken token = default)
            where T : class, ICacheItem
            => Task.FromResult(Maybe<T>.None);

        public Maybe<T> Find<T>(T value) where T : class, ICacheItem
            => Maybe<T>.None;

        public Task<Maybe<T>> FindAsync<T>(T value, CancellationToken token = default)
            where T : class, ICacheItem
            => Task.FromResult(Maybe<T>.None);

        public Maybe<T> FindBy<T>(Func<T, bool> predicate) where T : class, ICacheItem
            => Maybe<T>.None;
        public Task<Maybe<T>> FindByAsync<T>(Func<T, bool> predicate, CancellationToken token = default) where T : class, ICacheItem
            => Task.FromResult(Maybe<T>.None);

        public IEnumerable<string> GetKeys(string pattern = "*", int total = 1000)
            => Enumerable.Empty<string>();

        public Task<IEnumerable<string>> GetKeysAsync(string pattern = "*", int total = 1000,
            CancellationToken token = default) => Task.FromResult(Enumerable.Empty<string>());

        public IEnumerable<T> GetAll<T>(string pattern = "*", int maxCount = 1000) where T : class, ICacheItem
            => Enumerable.Empty<T>();

        public Task<IEnumerable<T>> GetAllAsync<T>(string pattern = "*", int maxCount = 1000,
            CancellationToken token = default) where T : class, ICacheItem
            => Task.FromResult(Enumerable.Empty<T>());

        public IPagedList<T> Get<T>(int pageNumber = 0, int pageSize = 100, int maxCount = 1000)
            where T : class, ICacheItem => PagedList<T>.Empty;

        public Task<IPagedList<T>> GetAsync<T>(int pageNumber = 0, int pageSize = 100, int maxCount = 1000,
            CancellationToken token = default)
            where T : class, ICacheItem => Task.FromResult(PagedList<T>.Empty);

        public IPagedList<T> Get<T>(IEnumerable<string> keys, int pageNumber = 0, int pageSize = 100)
            where T : class, ICacheItem
            => PagedList<T>.Empty;

        public Task<IPagedList<T>> GetAsync<T>(IEnumerable<string> keys, int pageNumber = 0, int pageSize = 100,
            CancellationToken token = default)
            where T : class, ICacheItem => Task.FromResult(PagedList<T>.Empty);

        public IEnumerable<T> GetBy<T>(Func<T, bool> predicate) where T : class, ICacheItem
            => Enumerable.Empty<T>();

        public Task<IEnumerable<T>> GetByAsync<T>(Func<T, bool> predicate, CancellationToken token = default)
            where T : class, ICacheItem
            => Task.FromResult(Enumerable.Empty<T>());

        public Result AddOrUpdate<T>(T[] value, TimeSpan? expiresIn = default) where T : class, ICacheItem
            => Result.Fail("Cache manager is not available");

        public Task<Result> AddOrUpdateAsync<T>(T[] value, TimeSpan? expiresIn = default,
            CancellationToken token = default) where T : class, ICacheItem
            => Task.FromResult(Result.Fail("Cache manager is not available"));

        public Result Update<T>(T[] value, TimeSpan? expiresIn = default)
            where T : class, ICacheItem
            => Result.Fail("Cache manager is not available");

        public Task<Result> UpdateAsync<T>(T[] value, TimeSpan? expiresIn = default,
            CancellationToken token = default) where T : class, ICacheItem
            => Task.FromResult(Result.Fail("Cache manager is not available"));

        public Result Remove<T>(T[] value) where T : class, ICacheItem
            => Result.Fail("Cache manager is not available");

        public Task<Result> RemoveAsync<T>(T[] value, CancellationToken token = default)
            where T : class, ICacheItem
            => Task.FromResult(Result.Fail("Cache manager is not available"));

        public Result RemoveAll()
            => Result.Fail("Cache manager is not available");

        public Task<Result> RemoveAllAsync(CancellationToken token = default)
            => Task.FromResult(Result.Fail("Cache manager is not available"));

        public Result RemoveBy(Func<string, bool> predicate)
            => Result.Fail("Cache manager is not available");

        public Task<Result> RemoveByAsync(Func<string, bool> predicate, CancellationToken token = default)
            => Task.FromResult(Result.Fail("Cache manager is not available"));
    }
}