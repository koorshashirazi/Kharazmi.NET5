using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Caching;
using Kharazmi.Collections;
using Kharazmi.Common.Caching;
using Kharazmi.Options.Redis;

namespace Kharazmi.Redis
{
    // TODO full text search via redis search indexing
    
    /// <summary>_ </summary>
    public interface IRedisCacheManager : ICacheManager
    {
        /// <summary>Default use first option of RedisDbOptions</summary>
        public RedisDbOption RedisOption { get; }

        /// <summary>To Switch between databases</summary>
        public IRedisCacheManager SetDatabaseTo(RedisDbOption option);


        /// <summary>_</summary>
        /// <param name="keys"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        IPagedList<ICacheItem> GetCacheItems(IEnumerable<string> keys, int page = 0, int pageSize = 10);

        /// <summary>_</summary>
        /// <param name="keys"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<IPagedList<ICacheItem>> GetCacheItemsAsync(IEnumerable<string> keys, int page = 0,
            int pageSize = 10, CancellationToken token = default);

        /// <summary>_</summary>
        /// <param name="pattern"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        IPagedList<ICacheItem> GetCacheItems(string pattern = "*", int page = 0, int pageSize = 10);

        /// <summary>_</summary>
        /// <param name="pattern"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<IPagedList<ICacheItem>> GetCacheItemsAsync(string pattern = "*", int page = 0, int pageSize = 10,
            CancellationToken token = default);
    }
}