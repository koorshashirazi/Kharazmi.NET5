#region

using System;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Dependency;
using Kharazmi.Functional;
using Kharazmi.Options.Mongo;
using MongoDB.Bson;
using MongoDB.Driver;

#endregion

namespace Kharazmi.Localization
{
    public interface IMongoDbContext: IMustBeInstance
    {
        IMongoDbContext SetDatabaseTo(MongoOption option);
        int GetCommandsCount { get; }
        int GetTransactionCommandsCount { get; }
        Task AddCommandAsync(string key, CommandHandler command);
        Task AddTransactionCommandAsync(string key, CommandTransactionHandler command);
        IMongoCollection<TEntity>? GetCollection<TEntity>(string? name = null);
        bool Exists(string collectionName);
        Task CreateCollectionAsync(string name, CreateCollectionOptions options);
        Task DropCollectionAsync(string name);
        TResult RunCommand<TResult>(string command);
        TResult RunCommand<TResult>(string command, ReadPreference readPreference);
        BsonValue RunScript(string command);
        Task<BsonValue> RunScriptAsync(string command, CancellationToken token = default);
        Result Commit();

        Task<Result> CommitAsync(CancellationToken token = default);
        
        Result CommitTransaction(Func<Task<Result>>? before = null, Func<Task<Result>>? after = null);

        Task<Result> CommitTransactionAsync(Func<Task<Result>>? before = null, Func<Task<Result>>? after = null,
            CancellationToken token = default);
    }
}