using System.Linq;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;

namespace Kharazmi.Localization.Conventions
{
    public class UnderscoreNamingConvention : ConventionBase, IMemberMapConvention
    {
        public void Apply(BsonMemberMap memberMap)
        {
            var name = memberMap.MemberName;
            name = string.Concat(name.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x : x.ToString())).ToLower();
            memberMap.SetElementName(name);
        }
    }
}