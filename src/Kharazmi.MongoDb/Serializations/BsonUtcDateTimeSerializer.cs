using System;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Kharazmi.Localization.Serializations
{
    public class BsonUtcDateTimeSerializer : DateTimeSerializer
    {
        public override DateTime Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
            => new DateTime(base.Deserialize(context, args).Ticks, DateTimeKind.Unspecified);

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, DateTime value)
            => base.Serialize(context, args, new DateTime(value.Ticks, DateTimeKind.Utc));
    }
}