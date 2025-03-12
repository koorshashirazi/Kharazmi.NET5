using Kharazmi.Dependency;
using Kharazmi.Options.Mongo;
using MongoDB.Driver;

namespace Kharazmi.Localization.Default
{
    internal class NullMongoClientPool : IMongoClientPool, INullInstance
    {
        public NullMongoClientPool()
        {
        }


        public MongoClient Client(MongoOption option)
            => new();
    }
}