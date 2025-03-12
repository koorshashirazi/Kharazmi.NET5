using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Bson.Serialization.Serializers;

namespace Kharazmi.Localization.Serializations
{
    public class StringDictionarySerializer : DictionarySerializerBase<Dictionary<string, string>>
    {
        public StringDictionarySerializer() : base(DictionaryRepresentation.Document)
        {
        }

        protected override Dictionary<string, string> CreateInstance()
        {
            return new Dictionary<string, string>();
        }

        public override void Serialize(
            BsonSerializationContext context,
            BsonSerializationArgs args,
            Dictionary<string, string>? value)
        {
            if (value is null) return;
            var dic = value.ToDictionary(d => d.Key, d => d.Value);
            BsonSerializer.Serialize(context.Writer, dic);
        }

        public override Dictionary<string, string> Deserialize(
            BsonDeserializationContext context,
            BsonDeserializationArgs args)
        {
            var dic = BsonSerializer.Deserialize<Dictionary<string, string>>(context.Reader);
            if (dic is null)
                return new Dictionary<string, string>();

            var ret = new Dictionary<string, string>();
            foreach (var pair in dic) ret[pair.Key] = pair.Value;

            return ret;
        }
    }
}