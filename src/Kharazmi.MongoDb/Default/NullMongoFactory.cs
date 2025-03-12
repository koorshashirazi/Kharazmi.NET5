using System;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Common.Domain;
using Kharazmi.Dependency;
using Kharazmi.Functional;
using Kharazmi.Options.Mongo;

namespace Kharazmi.Localization.Default
{
    public class NullMongoFactory : IMongoFactory, INullInstance
    {
        public IMongoFactory SetDatabaseTo(string optionKey)
        {
            return this;
        }


        public IMongoDbContext DbContext => new NullMongoDbContext();

        public int GetCommandsCount => 0;

        public int GetTransactionCommandsCount => 0;

        public IMongoFactory SetDatabaseTo(MongoOption option) => this;

        public IMongoRepository<TAggregateRoot> Repository<TAggregateRoot>()
            where TAggregateRoot : class, IAggregateRoot<string>
            => new NullMongoRepository<TAggregateRoot>();

        public IMongoRepository<TAggregateRootCache> Cache<TAggregateRootCache>()
            where TAggregateRootCache : class, IAggregateRootCache<string>
            => new NullMongoRepository<TAggregateRootCache>();

        public Result SaveChanges()
            => Result.Fail("Can't resolve any implementation of IMongoFactory");

        public Task<Result> SaveChangesAsync(CancellationToken token = default)
            => Task.FromResult(Result.Fail("Can't resolve any implementation of IMongoFactory"));

        public Result SaveChanges(Func<Task<Result>>? before, Func<Task<Result>>? after = null)
            => Result.Fail("Can't resolve any implementation of IMongoFactory");

        public Task<Result> SaveChangesAsync(Func<Task<Result>>? before,
            Func<Task<Result>>? after = null,
            CancellationToken token = default) =>
            Task.FromResult(Result.Fail("Can't resolve any implementation of IMongoFactory"));
    }
}