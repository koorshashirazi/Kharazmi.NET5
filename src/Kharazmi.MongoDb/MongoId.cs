#region

using System;
using Kharazmi.Common.Json;
using Kharazmi.Common.ValueObjects;
using Newtonsoft.Json;

#endregion

namespace Kharazmi.Localization
{
    [JsonConverter(typeof(SingleValueObjectConverter))]
    public class MongoId : Identity<MongoId, string>
    {
        public MongoId(string value) : base(value)
        {
        }

        public static MongoId Empty => new(Guid.Empty.ToString("N"));
        public static MongoId New => new(Guid.NewGuid().ToString("N"));
    }
}