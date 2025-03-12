using Kharazmi.Common.Caching;
using Kharazmi.Extensions;
using Kharazmi.Helpers;
using Newtonsoft.Json;

namespace Kharazmi.Bus
{
    public class BusMessageStored : CacheItem
    {
        public static string DefaultBusMessageStoredScheme = "BusMessageStored";

        [JsonConstructor]
        public BusMessageStored(string messageId, string? nameSpace = null)
        {
            Created = DateTimeHelper.DateTimeOffsetUtcNow.ToString("O");
            Updated = DateTimeHelper.DateTimeOffsetUtcNow.ToString("O");
            MessageId = messageId;
            NameSpace = nameSpace;
        }

        public string MessageId { get; }
        public string? NameSpace { get; }
        public string Created { get; }
        public string Updated { get; private set; }

        public BusMessageStored UpdateDateTime()
        {
            Updated = DateTimeHelper.DateTimeOffsetUtcNow.ToString("O");
            return this;
        }

        public override ICacheItem BuildCacheKey()
        {
            var key = NameSpace.IsNotEmpty()
                ? $"{DefaultBusMessageStoredScheme}:{NameSpace}:{MessageId}"
                : $"{DefaultBusMessageStoredScheme}:{MessageId}";
            UpdateCacheItemKey(key);
            return this;
        }
    }
}