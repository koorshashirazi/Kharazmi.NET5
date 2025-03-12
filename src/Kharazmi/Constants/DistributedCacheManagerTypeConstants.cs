using System.Collections.Generic;

namespace Kharazmi.Constants
{
    public static class DistributedCacheManagerTypeConstants
    {
        public static HashSet<string> Providers => new()
        {
            RedisDb,
            MongoDb,
            HostMemory,
            ModeLess
        };

        public const string ModeLess = "ModeLess";
        public const string HostMemory = "HostMemory";
        public const string MongoDb = "MongoDb";
        public const string RedisDb = "RedisDb";
    }
}