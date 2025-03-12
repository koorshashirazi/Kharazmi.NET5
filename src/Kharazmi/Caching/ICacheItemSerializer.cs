using Kharazmi.Common.Caching;
using Kharazmi.Common.ValueObjects;

namespace Kharazmi.Caching
{
    public interface ICacheItemSerializer
    {
        Option<ICacheItem> DeserializeCacheItem(byte[]? serializedObject);
    }
}