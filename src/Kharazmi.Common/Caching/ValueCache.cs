#region

using System;
using Kharazmi.Common.Domain;
using Kharazmi.Common.Events;
using Newtonsoft.Json;

#endregion

namespace Kharazmi.Common.Caching
{
    /// <summary>_</summary>
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class ValueCache : AggregateRoot<string>
    {
        private const long NotPresent = -1;

        /// <summary>_</summary>
        public byte[]? Value { get; set; }

        /// <summary>_</summary>
        public DateTime? AbsoluteExpiration { get; set; }

        /// <summary>
        /// Gets or sets how long a cache entry can be inactive (e.g. not accessed) before it will be removed.
        /// </summary>
        public TimeSpan? SlidingExpiration { get; set; }

        /// <summary>_</summary>
        /// <param name="id"></param>
        [JsonConstructor]
        public ValueCache(CacheId id) : base(id)
        {
        }

        /// <summary>_</summary>
        /// <param name="utcNow"></param>
        /// <returns></returns>
        public bool IsExpired(DateTime utcNow) => AbsoluteExpiration.HasValue && AbsoluteExpiration.Value < utcNow;

        /// <summary>_</summary>
        /// <param name="utcNow"></param>
        /// <param name="options"></param>
        public void SetExpirationFrom(DateTime utcNow, CacheEntryOptions options)
        {
            if (options.AbsoluteExpiration.HasValue && options.AbsoluteExpiration <= utcNow)
            {
                throw new ArgumentOutOfRangeException(nameof(AbsoluteExpiration), options.AbsoluteExpiration.Value,
                    "The absolute expiration value must be in the future.");
            }

            if (options.AbsoluteExpiration.HasValue)
                AbsoluteExpiration = new DateTime(options.AbsoluteExpiration.Value.Ticks, DateTimeKind.Utc);

            if (options.AbsoluteExpirationRelativeToNow.HasValue)
                AbsoluteExpiration = utcNow.Add(options.AbsoluteExpirationRelativeToNow.Value);

            if (options.SlidingExpiration.HasValue)
                SlidingExpiration = options.SlidingExpiration.Value;

            if (!AbsoluteExpiration.HasValue && SlidingExpiration.HasValue)
                AbsoluteExpiration = utcNow.Add(SlidingExpiration.Value);
        }

        /// <summary>_</summary>
        /// <returns></returns>
        public long GetExpirationTicks() => AbsoluteExpiration?.Ticks ?? NotPresent;

        /// <summary>_</summary>
        /// <returns></returns>
        public long GetSlidingTicks() => SlidingExpiration?.Ticks ?? NotPresent;

        /// <summary>_</summary>
        /// <param name="utcNow"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public long GetExpirationInSeconds(DateTime utcNow, CacheEntryOptions options)
        {
            return AbsoluteExpiration.HasValue switch
            {
                true when !(AbsoluteExpiration is null) && options.SlidingExpiration.HasValue => (long) Math.Min(
                    (AbsoluteExpiration.Value - utcNow).TotalSeconds, options.SlidingExpiration.Value.TotalSeconds),
                true when !(AbsoluteExpiration is null) &&
                          !options.SlidingExpiration.HasValue =>
                    (long) (AbsoluteExpiration.Value - utcNow).TotalSeconds,
                false when options.SlidingExpiration.HasValue => (long) options.SlidingExpiration.Value.TotalSeconds,
                _ => NotPresent
            };
        }

        /// <summary>_</summary>
        /// <param name="utcNow"></param>
        /// <returns></returns>
        public TimeSpan? GetRealExpirationTimeSpan(DateTime utcNow)
        {
            if (!AbsoluteExpiration.HasValue)
                return SlidingExpiration;
            
            TimeSpan? relExpiration = AbsoluteExpiration.Value - utcNow;
            if (!SlidingExpiration.HasValue) return relExpiration;

            return relExpiration <= SlidingExpiration.Value ? relExpiration : SlidingExpiration.Value;

        }


        /// <summary>_</summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ValueCache UpdateAbsoluteExpiration(DateTimeOffset value)
        {
            AbsoluteExpiration = new DateTime(value.Ticks);
            return this;
        }

        /// <summary>_</summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ValueCache UpdateAbsoluteExpiration(DateTime value)
        {
            AbsoluteExpiration = value;
            return this;
        }


        protected override void EnsureValidState()
        {
        }

        protected override void ApplyWhen(IAggregateEvent @event)
        {
        }
    }
}