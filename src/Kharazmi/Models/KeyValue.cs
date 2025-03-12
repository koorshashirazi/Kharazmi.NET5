using Newtonsoft.Json;

namespace Kharazmi.Models
{
    public record KeyValue<TKey, TValue>
    {
        public KeyValue()
        {
        }

        [System.Text.Json.Serialization.JsonConstructor, JsonConstructor]
        public KeyValue(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }

        public TKey Key { get; set; }
        public TValue Value { get; set; }
    }
}