using System;
using System.Text;
using Kharazmi.Caching;
using Kharazmi.Channels;
using Kharazmi.Common.Caching;
using Kharazmi.Common.Events;
using Kharazmi.Common.ValueObjects;
using Kharazmi.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kharazmi.Redis
{
    internal class RedisJsonSerializer : StackExchange.Redis.Extensions.Core.ISerializer,
        IChannelJsonSerializer, ICacheItemSerializer
    {
        private static readonly Encoding Encoding = Encoding.UTF8;
        private static readonly JsonSerializerSettings JsonSerializerSettings = Serializer.DefaultJsonSettings;

        static RedisJsonSerializer()
        {
            JsonSerializerSettings.NullValueHandling = NullValueHandling.Include;
        }
        public RedisJsonSerializer()
        {
        }


        public byte[] Serialize(object? item)
        {
            if (item is null) return null!;
            var jsonString = item.Serialize(JsonSerializerSettings);
            return Encoding.GetBytes(jsonString);
        }

        public byte[]? SerializeEvent(object? item)
        {
            if (item is null) return null;

            var type = item.GetType();
            if (!type.IsAssignableFrom(typeof(IDomainEvent))) return Encoding.GetBytes("");
            var jsonString = item.Serialize(type, JsonSerializerSettings);
            return Encoding.GetBytes(jsonString);
        }

        public T Deserialize<T>(byte[]? serializedObject)
        {
            if (serializedObject is null) return default!;

            try
            {
                var jsonString = Encoding.GetString(serializedObject);
                var result = jsonString.Deserialize<T>(JsonSerializerSettings);
                return result ?? Activator.CreateInstance<T>();
            }
            catch
            {
                return default!;
            }
        }

        public Option<ICacheItem> DeserializeCacheItem(byte[]? serializedObject)
        {
            if (serializedObject is null) return new None<ICacheItem>();
            var jsonString = Encoding.GetString(serializedObject);
            var obj = jsonString.Deserialize(JsonSerializerSettings);
            if (obj is null)
                return new None<ICacheItem>();

            var obj2 = JObject.FromObject(obj);

            var cacheType = "";

            foreach (var (key, value) in obj2)
            {
                if (key != nameof(ICacheItem.CacheType)) continue;
                cacheType = value?.ToObject<string>();
                break;
            }

            if (string.IsNullOrWhiteSpace(cacheType))
                return new None<ICacheItem>();

            if (jsonString.Deserialize(Type.GetType(cacheType)) is ICacheItem result)
                return new Some<ICacheItem>(result);
            return new None<ICacheItem>();
        }
    }
}