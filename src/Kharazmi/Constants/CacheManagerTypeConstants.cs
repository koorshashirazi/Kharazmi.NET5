using System.Collections.Generic;

namespace Kharazmi.Constants
{
    public static class CacheManagerTypeConstants
    {
        public static HashSet<string> Providers => new()
        {
            ModeLess,
            HostMemory,
            Distributed,
            RedisDb
        };

        public const string ModeLess = "ModeLess";
        public const string HostMemory = "HostMemory";
        public const string Distributed = "Distributed";
        public const string RedisDb = "RedisDb";
    }
}