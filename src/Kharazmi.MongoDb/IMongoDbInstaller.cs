using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Dependency;
using Kharazmi.Options.Mongo;

namespace Kharazmi.Localization
{
    public interface IMongoDbInstaller : IShouldBeSingleton
    {
        string AssemblyDbInstaller { get; }
        IMongoDbInstaller SetDatabaseTo(MongoOption option);
        void CreateTables();
        Task CreateTablesAsync(CancellationToken token = default);
        void CreateIndexes();
        Task CreateIndexesAsync(CancellationToken token = default);
        void Seed();
        Task SeedAsync(CancellationToken token = default);
        void Execute();
        Task ExecuteAsync(CancellationToken token = default);
        
    }
}