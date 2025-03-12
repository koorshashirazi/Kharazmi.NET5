#region

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Configuration;
using Kharazmi.Dependency;
using Kharazmi.Functional;
using Kharazmi.Localization.Extensions;
using Kharazmi.Options.Mongo;
using Kharazmi.Threading;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Core.Clusters;
using MongoDB.Driver.Core.Operations;

#endregion

namespace Kharazmi.Localization
{
    public delegate Task CommandHandler(CancellationToken token = default);

    public delegate Task CommandTransactionHandler(IClientSessionHandle sessionHandle,
        CancellationToken token = default);

    public class MongoDbContext : IMongoDbContext, IDisposable
    {
        #region Fields

        private readonly ISettingProvider _settingProvider;
        private readonly ILogger<MongoDbContext>? _logger;
        private readonly IMongoClientPool _mongoClientPool;
        private readonly ConcurrentDictionary<string, CommandHandler> _commands;
        private readonly ConcurrentDictionary<string, CommandTransactionHandler> _transactionCommands;
        private MongoOption _option;
        private IMongoDatabase _database;
        private IMongoClient _client;

        #endregion

        #region Ctor

        public MongoDbContext(
            ISettingProvider settingProvider,
            ServiceFactory<IMongoClientPool> factory,
            [MaybeNull] ILogger<MongoDbContext>? logger)
        {
            _settingProvider = settingProvider;
            _option = _settingProvider.GetMongoOption("");
            _mongoClientPool = factory.Instance();
            _logger = logger;
            _commands = new ConcurrentDictionary<string, CommandHandler>();
            _transactionCommands = new ConcurrentDictionary<string, CommandTransactionHandler>();
            _client = _mongoClientPool.Client(_option);
            _database = _client.GetDatabase(_option.Database);
            // Client.Cluster.DescriptionChanged += ClusterOnDescriptionChanged;
        }

        #endregion

        #region Methods

        public IMongoDbContext SetDatabaseTo(MongoOption option)
        {
            _option = option;
            _client = _mongoClientPool.Client(option);
            _database = _client.GetDatabase(option.Database);
            return this;
        }

        public int GetCommandsCount => _commands.Count;
        public int GetTransactionCommandsCount => _transactionCommands.Count;

        public IMongoCollection<TEntity>? GetCollection<TEntity>(string? name = null)
        {
            try
            {
                name ??= typeof(TEntity).Name;
                return _database.GetCollection<TEntity>(name);
            }
            catch (Exception e)
            {
                _logger?.LogError("{Message}", e.Message);
                return default;
            }
        }

        public async Task AddCommandAsync(string key, CommandHandler command)
        {
            _commands[key] = command;
            await Task.CompletedTask;
        }

        public async Task AddTransactionCommandAsync(string key, CommandTransactionHandler command)
        {
            _transactionCommands[key] = command;
            await Task.CompletedTask;
        }

        private void RemoveCommand(string key, bool isTransactionCommand = false)
        {
            if (isTransactionCommand)
                _transactionCommands.TryRemove(key, out _);
            else
                _commands.TryRemove(key, out _);
        }

        private void RemoveCommands(IEnumerable<string> keys, bool isTransactionCommand = false)
        {
            foreach (var key in keys)
                RemoveCommand(key, isTransactionCommand);
        }

        private void RemoveCommands(bool isTransactionCommand = false)
        {
            if (isTransactionCommand)
                _transactionCommands.Clear();
            else
                _commands.Clear();
        }


        public Task CreateCollectionAsync(string name, CreateCollectionOptions options)
        {
            try
            {
                _logger?.LogTrace("Create Collection: {Name}", name);

                return _database.CreateCollectionAsync(name, options);
            }
            catch (Exception e)
            {
                _logger?.LogError("{Message}", e.Message);
                return Task.CompletedTask;
            }
        }

        public Task DropCollectionAsync(string name)
        {
            try
            {
                _logger?.LogTrace("Drop Collection: {Name}", name);
                return _database.DropCollectionAsync(name);
            }
            catch (Exception e)
            {
                _logger?.LogError("{Message}", e.Message);
                return Task.CompletedTask;
            }
        }

        public Result Commit()
            => AsyncHelper.RunSync(() => CommitAsync(CancellationToken.None));

        public async Task<Result> CommitAsync(CancellationToken token = default)
        {
            try
            {
                if (GetCommandsCount <= 0) return Result.Ok(0);

                token.ThrowIfCancellationRequested();

                // TODO ExecuteHooks<IPreActionHook>(entryList);

                _logger?.LogTrace("Commiting: {GetCommandsCount}", GetCommandsCount);

                foreach (var command in _commands)
                    await command.Value(token);

                _logger?.LogTrace("Committed changes");

                RemoveCommands();

                // TODO ExecuteHooks<IPostActionHook>(entryList);

                return GetCommandsCount == 0 ? Result.Ok() : Result.Fail("Failed Commit changes");
            }
            catch (Exception e)
            {
                _logger?.LogError("{Message}", e.Message);
                return Result.Fail("Failed Commit changes");
            }
        }

        public Result CommitTransaction(Func<Task<Result>>? before = null, Func<Task<Result>>? after = null)
            => AsyncHelper.RunAsSync(() => CommitTransactionAsync(before, after));


        public async Task<Result> CommitTransactionAsync(Func<Task<Result>>? before = null,
            Func<Task<Result>>? after = null, CancellationToken token = default)
        {
            if (GetTransactionCommandsCount <= 0) return Result.Ok(0);

            using var sessionHandle = await _client.StartSessionAsync(new ClientSessionOptions(), token);
            try
            {
                token.ThrowIfCancellationRequested();
                sessionHandle.StartTransaction();

                _logger?.LogTrace("Commiting: {GetTransactionCommandsCount}", GetTransactionCommandsCount);

                Result result;

                if (before != null)
                {
                    result = await before.Invoke();
                    if (result.Failed) return result;
                }

                foreach (var command in _transactionCommands)
                    await command.Value(sessionHandle, token);

                if (after != null)
                {
                    result = await after.Invoke();
                    if (result.Failed)
                        throw new MongoDbException(result.ToString());
                }

                await sessionHandle.CommitTransactionAsync(token);

                RemoveCommands(true);

                _logger?.LogTrace("Committed changes {TransactionCommands}",
                    string.Join(Environment.NewLine, _transactionCommands.Keys.ToArray()));

                return GetTransactionCommandsCount == 0 ? Result.Ok() : Result.Fail("Failed Commit changes");
            }
            catch (Exception e)
            {
                RemoveCommands(true);
                await sessionHandle.AbortTransactionAsync(token);
                _logger?.LogError("{Message}", e.Message);
                return Result.Fail("Can't commit changes");
            }
        }


        public TResult RunCommand<TResult>(string command)
        {
            try
            {
                return _database.RunCommand<TResult>(command);
            }
            catch (Exception e)
            {
                _logger?.LogError("{Message}", e.Message);
                return default!;
            }
        }

        public TResult RunCommand<TResult>(string command, ReadPreference readPreference)
        {
            try
            {
                return _database.RunCommand<TResult>(command, readPreference);
            }
            catch (Exception e)
            {
                _logger?.LogError("{Message}", e.Message);
                return default!;
            }
        }

        public BsonValue RunScript(string command)
        {
            return AsyncHelper.RunSync(() => RunScriptAsync(command));
        }

        public Task<BsonValue> RunScriptAsync(string command, CancellationToken token = default)
        {
            try
            {
                var script = new BsonJavaScript(command);
                var operation = new EvalOperation(_database.DatabaseNamespace, script, null);
                var writeBinding = new WritableServerBinding(_database.Client.Cluster, NoCoreSession.NewHandle());
                return operation.ExecuteAsync(writeBinding, token);
            }
            catch (Exception e)
            {
                _logger?.LogError("{Message}", e.Message);
                return default!;
            }
        }

        public bool Exists(string collectionName)
        {
            try
            {
                var filter = new BsonDocument("name", collectionName);
                var options = new ListCollectionNamesOptions {Filter = filter};
                return _database.ListCollectionNames(options).Any();
            }
            catch (Exception e)
            {
                _logger?.LogError("{Message}", e.Message);
                return false;
            }
        }

        #endregion

        private void ClusterOnDescriptionChanged(object sender, ClusterDescriptionChangedEventArgs e)
        {
            _logger?.LogTrace(
                "MongoDbContext description changed with db options type {MongoOption}, clusterId {ClusterId}, commands counts {CommandsCount}, command transaction counts: {TransactionCommandsCount}",
                _option.OptionKey,
                e.NewClusterDescription.ClusterId,
                GetCommandsCount,
                GetTransactionCommandsCount);
        }

        public void Dispose()
        {
            _commands.Clear();
            _transactionCommands.Clear();
        }
    }
}