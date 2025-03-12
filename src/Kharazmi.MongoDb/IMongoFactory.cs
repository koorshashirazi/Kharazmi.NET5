using System;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Common.Domain;
using Kharazmi.Dependency;
using Kharazmi.Functional;
using Kharazmi.Options.Mongo;

namespace Kharazmi.Localization
{
    public interface IMongoFactory : IMustBeInstance
    {
        /// <summary>To Switch between databases</summary>
        IMongoDbContext DbContext { get; }

        int GetCommandsCount { get; }
        int GetTransactionCommandsCount { get; }

        IMongoFactory SetDatabaseTo(MongoOption option);

        IMongoRepository<TAggregateRoot> Repository<TAggregateRoot>()
            where TAggregateRoot : class, IAggregateRoot<string>;

        IMongoRepository<TAggregateRootCache> Cache<TAggregateRootCache>()
            where TAggregateRootCache : class, IAggregateRootCache<string>;

        Result SaveChanges();
        Task<Result> SaveChangesAsync(CancellationToken token = default);

        Result SaveChanges(Func<Task<Result>>? before, Func<Task<Result>>? after = null);

        Task<Result> SaveChangesAsync(Func<Task<Result>>? before,
            Func<Task<Result>>? after = null,
            CancellationToken token = default);
    }
}