using System;
using System.Collections.Generic;
using System.Linq;
using Kharazmi.Common.Extensions;
using Kharazmi.Common.Json;

namespace Kharazmi.Common.Metadata
{
    public class MetadataCollection : Dictionary<string, string>
    {
        public MetadataCollection()
        {
        }

        public MetadataCollection(IDictionary<string, string> value) : base(value)
        {
        }

        public MetadataCollection(IEnumerable<KeyValuePair<string, string>> value)
            : base(value.ToDictionary(kv => kv.Key, kv => kv.Value))
        {
        }

        public MetadataCollection(params KeyValuePair<string, string>[] value)
            : this((IEnumerable<KeyValuePair<string, string>>) value)
        {
        }


        public virtual MetadataCollection AddRange(params KeyValuePair<string, string>[] value)
            => AddRange((IEnumerable<KeyValuePair<string, string>>) value);

        public virtual MetadataCollection AddRange(IEnumerable<KeyValuePair<string, string>>? value)
        {
            if (value is null) return this;
            foreach (var kv in value)
            {
                try
                {
                    Add(kv.Key, kv.Value);
                }
                catch
                {
                    throw new InvalidOperationException(
                        $"Can't add eventMetadata with key {kv.Key}.There is already some with this key");
                }
            }

            return this;
        }

        public virtual MetadataCollection TryAddRange(params KeyValuePair<string, string>[] value)
            => TryAddRange((IEnumerable<KeyValuePair<string, string>>) value);

        public virtual MetadataCollection TryAddRange(IEnumerable<KeyValuePair<string, string>>? value)
        {
            if (value is null) return this;
            foreach (var kv in value)
            {
                try
                {
                    Add(kv.Key, kv.Value);
                }
                catch
                {
                    // ignored
                }
            }

            return this;
        }


        public virtual MetadataCollection AddOrUpdateRange(params KeyValuePair<string, string>[] value)
            => AddOrUpdateRange((IEnumerable<KeyValuePair<string, string>>) value);

        public virtual MetadataCollection AddOrUpdateRange(IEnumerable<KeyValuePair<string, string>> value)
        {
            foreach (var kv in value)
            {
                try
                {
                    var key = kv.Key;
                    var hsValue = HasValue(key);
                    if (hsValue)
                        this[key] = kv.Value;
                    else
                        Add(key, kv.Value);
                }
                catch
                {
                    // ignored
                }
            }

            return this;
        }

        public bool HasValue(string key)
        {
            try
            {
                return !(GetValue(key) is null);
            }
            catch
            {
                return false;
            }
        }


        public virtual string GetValue(string key)
            => GetValue(key, s => s);

        public virtual T GetValue<T>(string key, Func<string, T> mapper)
        {
            try
            {
                TryGetValue(key, out var value);

                if (!(value is null)) return mapper.Invoke(value);

                key = key.FirstToLower();
                TryGetValue(key, out var value2);

                if (value2 is null)
                    throw new KeyNotFoundException($"Can't find eventMetadata value with key {key}");

                return mapper.Invoke(value2);
            }
            catch (Exception e)
            {
                throw new InvalidCastException(e.Message);
            }
        }


        public virtual string TryGetValue(string key)
            => TryGetValue(key, s => s);

        public virtual T TryGetValue<T>(string key, Func<string, T> mapper)
        {
            try
            {
                return GetValue(key, mapper);
            }
            catch
            {
                return default;
            }
        }

        public virtual IReadOnlyDictionary<string, string> GetAll() => this;

        public string JsonString() => this.Serialize();

        public static MetadataCollection? FromJson(string json) => json.Deserialize<MetadataCollection>();

        public override string ToString()
        {
            var keyValues = string.Join(",", this.Select(x => $"[{x.Key}: {x.Value}]"));
            return $"{Environment.NewLine}{keyValues}{Environment.NewLine}";
        }

    }
}