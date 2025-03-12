using System;

namespace Kharazmi.Common.Caching
{
    /// <summary>_</summary>
    public class CacheEntryOptions
    {
        /// <summary>_</summary>
        /// <param name="absoluteExpiration"></param>
        /// <param name="absoluteExpirationRelativeToNow"></param>
        /// <param name="slidingExpiration"></param>
        public CacheEntryOptions(
            DateTimeOffset? absoluteExpiration, 
            TimeSpan? absoluteExpirationRelativeToNow,
            TimeSpan? slidingExpiration)
        {
            AbsoluteExpiration = absoluteExpiration;
            AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow;
            SlidingExpiration = slidingExpiration;
        }

        /// <summary>
        /// Gets or sets an absolute expiration date for the cache entry.
        /// </summary>
        public DateTimeOffset? AbsoluteExpiration { get; set; }

        /// <summary>
        /// Gets or sets an absolute expiration time, relative to now.
        /// </summary>
        public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }

        /// <summary>
        /// Gets or sets how long a cache entry can be inactive (e.g. not accessed) before it will be removed.
        /// This will not extend the entry lifetime beyond the absolute expiration (if set).
        /// </summary>
        public TimeSpan? SlidingExpiration { get; set; }
    }
}