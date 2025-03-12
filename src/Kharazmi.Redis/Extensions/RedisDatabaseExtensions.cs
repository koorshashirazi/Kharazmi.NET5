using System.Threading.Tasks;
using Kharazmi.Configuration;
using Kharazmi.Exceptions;
using Kharazmi.Extensions;
using Kharazmi.Options;
using Kharazmi.Options.Redis;
using Kharazmi.Threading;
using StackExchange.Redis;

namespace Kharazmi.Redis.Extensions
{
    /// <summary>_</summary>
    public static class RedisDatabaseExtensions
    {
        private const string HmGetScript = @"return redis.call('HMGET', KEYS[1], unpack(ARGV))";

        /// <summary>_</summary>
        /// <param name="cache"></param>
        /// <param name="key"></param>
        /// <param name="members"></param>
        /// <returns></returns>
        public static RedisValue[]? HashMemberGet(this IDatabase cache, string key, params string[] members)
            => AsyncHelper.RunSync(() => HashMemberGetAsync(cache, key, members));

        /// <summary>_</summary>
        /// <param name="cache"></param>
        /// <param name="key"></param>
        /// <param name="members"></param>
        /// <returns></returns>
        public static async Task<RedisValue[]?> HashMemberGetAsync(
            this IDatabase cache,
            string key,
            params string[] members)
        {
            var result = await cache.ScriptEvaluateAsync(
                HmGetScript,
                new RedisKey[] {key},
                GetRedisMembers(members)).ConfigureAwait(false);

            if (!result.IsNull)
                return (RedisValue[]) result;
            return null;
        }

        /// <summary>_</summary>
        /// <param name="settingProvider"></param>
        /// <param name="optionKey"></param>
        /// <returns></returns>
        /// <exception cref="NotFoundOptionException"></exception>
        public static RedisDbOption GetRedisOption(this ISettingProvider settingProvider, string? optionKey)
        {
            var cacheOption = settingProvider.Get<CacheOption>();
            var options = settingProvider.Get<RedisDbOptions>();
            var key = optionKey.IsEmpty()
                ? options.DefaultOption
                : optionKey;

            var option = options.FindOrNone(x =>
                x.OptionKey == key && x.OptionKey != cacheOption.DistributedProviderOptionKey);

            if (option.IsNull())
                throw new NotFoundOptionException(nameof(RedisDbOptions), optionKey);
            return option;
        }

    

        private static RedisValue[] GetRedisMembers(params string[] members)
        {
            var redisMembers = new RedisValue[members.Length];
            for (var i = 0; i < members.Length; i++)
            {
                redisMembers[i] = (RedisValue) members[i];
            }

            return redisMembers;
        }
    }
}