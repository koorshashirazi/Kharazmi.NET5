using Kharazmi.Dependency;
using Kharazmi.Options.Mongo;
using MongoDB.Driver;

namespace Kharazmi.Localization
{
    public interface IMongoClientPool : IShouldBeSingleton, IMustBeInstance
    {
        MongoClient Client(MongoOption option);
    }
}