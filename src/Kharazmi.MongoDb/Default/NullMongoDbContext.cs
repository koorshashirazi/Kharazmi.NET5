using System;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Dependency;
using Kharazmi.Functional;
using Kharazmi.Options.Mongo;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Kharazmi.Localization.Default
{
    internal class NullMongoDbContext : IMongoDbContext, INullInstance
    {
        #region Fields

        #endregion

        #region Ctor

        public NullMongoDbContext()
        {
        }

        #endregion

        #region Methods

        public IMongoDbContext SetDatabaseTo(MongoOption option)
        {
            return this;
        }

        public int GetCommandsCount => 0;

        public int GetTransactionCommandsCount => 0;

        public IMongoCollection<TEntity>? GetCollection<TEntity>(string? name)
            => default;

        public Task AddCommandAsync(string key, CommandHandler command)
            => Task.CompletedTask;

        public Task AddTransactionCommandAsync(string key, CommandTransactionHandler command)
            => Task.CompletedTask;

        public Task CreateCollectionAsync(string name, CreateCollectionOptions options)
            => Task.CompletedTask;

        public Task DropCollectionAsync(string name)
            => Task.CompletedTask;

        public Result Commit()
            => Result.Fail("Can't resolve any implementation of IMongoDbContext");

        public Task<Result> CommitAsync(CancellationToken token = default)
            => Task.FromResult(Result.Fail("Can't resolve any implementation of IMongoDbContext"));

        public Result CommitTransaction(Func<Task<Result>>? before = null, Func<Task<Result>>? after = null)
            => Result.Fail("");


        public Task<Result> CommitTransactionAsync(Func<Task<Result>>? before = null, Func<Task<Result>>? after = null,
            CancellationToken token = default)
            => Task.FromResult(Result.Fail("Can't resolve any implementation of IMongoDbContext"));


        public TResult RunCommand<TResult>(string command)
            => default!;

        public TResult RunCommand<TResult>(string command, ReadPreference readPreference)
            => default!;

        public BsonValue RunScript(string command)
            => default!;

        public Task<BsonValue> RunScriptAsync(string command, CancellationToken token = default)
            => Task.FromResult<BsonValue>(default!);

        public bool Exists(string collectionName)
            => true;

        #endregion
    }
}