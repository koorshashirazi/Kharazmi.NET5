using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Collections;
using Kharazmi.Common.Caching;
using Kharazmi.Common.ValueObjects;
using Kharazmi.Dependency;
using Kharazmi.Functional;

namespace Kharazmi.Caching
{
    public interface ICacheManager : IDisposable, IMustBeInstance
    {
        Result<bool> Exist(string key);
        Task<Result<bool>> ExistAsync(string key, CancellationToken token = default);

        Result<bool> Exist<T>(T value) where T : class, ICacheItem;
        Task<Result<bool>> ExistAsync<T>(T value, CancellationToken token = default) where T : class, ICacheItem;
        Maybe<object> Find(string key);
        Task<Maybe<object>> FindAsync(string key, CancellationToken token = default);

        Maybe<T> Find<T>(string key) where T : class, ICacheItem;

        Task<Maybe<T>> FindAsync<T>(string key, CancellationToken token = default)
            where T : class, ICacheItem;

        Maybe<T> Find<T>(T value) where T : class, ICacheItem;

        Task<Maybe<T>> FindAsync<T>(T value, CancellationToken token = default)
            where T : class, ICacheItem;

        Maybe<T> FindBy<T>(Func<T, bool> predicate) where T : class, ICacheItem;

        Task<Maybe<T>> FindByAsync<T>(Func<T, bool> predicate, CancellationToken token = default)
            where T : class, ICacheItem;

        IEnumerable<string> GetKeys(string pattern = "*", int maxCount = 1000);

        Task<IEnumerable<string>> GetKeysAsync(string pattern = "*", int maxCount = 1000,
            CancellationToken token = default);

        IEnumerable<T> GetAll<T>(string pattern = "*", int maxCount = 1000) where T : class, ICacheItem;

        Task<IEnumerable<T>> GetAllAsync<T>(string pattern = "*", int maxCount = 1000,
            CancellationToken token = default) where T : class, ICacheItem;

        IPagedList<T> Get<T>(int pageNumber = 0, int pageSize = 100, int maxCount = 1000) where T : class, ICacheItem;

        Task<IPagedList<T>> GetAsync<T>(int pageNumber = 0, int pageSize = 100, int maxCount = 1000,
            CancellationToken token = default) where T : class, ICacheItem;

        IPagedList<T> Get<T>(IEnumerable<string> keys, int pageNumber = 0, int pageSize = 100)
            where T : class, ICacheItem;

        Task<IPagedList<T>> GetAsync<T>(IEnumerable<string> keys, int pageNumber = 0, int pageSize = 100,
            CancellationToken token = default) where T : class, ICacheItem;

        IEnumerable<T> GetBy<T>(Func<T, bool> predicate) where T : class, ICacheItem;

        Task<IEnumerable<T>> GetByAsync<T>(Func<T, bool> predicate, CancellationToken token = default)
            where T : class, ICacheItem;

        Result AddOrUpdate<T>(T[] value, TimeSpan? expiresIn = default) where T : class, ICacheItem;

        Task<Result> AddOrUpdateAsync<T>(T[] value, TimeSpan? expiresIn = default,
            CancellationToken token = default)
            where T : class, ICacheItem;

        Result Update<T>(T[] value, TimeSpan? expiresIn = default)
            where T : class, ICacheItem;

        Task<Result> UpdateAsync<T>(T[] value, TimeSpan? expiresIn = default,
            CancellationToken token = default)
            where T : class, ICacheItem;

        Result Remove<T>(T[] value) where T : class, ICacheItem;
        Task<Result> RemoveAsync<T>(T[] value, CancellationToken token = default) where T : class, ICacheItem;
        Result RemoveAll();
        Task<Result> RemoveAllAsync(CancellationToken token = default);

        Result RemoveBy(Func<string, bool> predicate);
        Task<Result> RemoveByAsync(Func<string, bool> predicate, CancellationToken token = default);
    }
}