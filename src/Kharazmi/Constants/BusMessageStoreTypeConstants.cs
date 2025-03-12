using System.Collections.Generic;

namespace Kharazmi.Constants
{
    public static class BusMessageStoreTypeConstants
    {
        public static IReadOnlyCollection<string> GetStorageType() => new[]
        {
            None,
            HostMemory,
            DistributedCache
        };
        
        public const string None = "None";
        public const string HostMemory = "HostMemory";
        public const string DistributedCache = "DistributedCache";
    }
}