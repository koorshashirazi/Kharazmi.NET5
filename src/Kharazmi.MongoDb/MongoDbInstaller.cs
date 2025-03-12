using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Common.Domain;
using Kharazmi.Configuration;
using Kharazmi.Constants;
using Kharazmi.Dependency;
using Kharazmi.Extensions;
using Kharazmi.Localization.Extensions;
using Kharazmi.Options.Mongo;
using Kharazmi.Threading;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace Kharazmi.Localization
{
    public abstract class MongoDbInstaller : IMongoDbInstaller
    {
        private readonly SemaphoreSlim _asyncLock = new(1);

        protected MongoDbInstaller(
            ILoggerFactory loggerFactory,
            ISettingProvider settings,
            ServiceFactory<IMongoFactory> factory)
        {
            Settings = settings;
            Factory = factory.Instance();
            _mongoOption = settings.GetMongoOption("");
            Logger = loggerFactory.CreateLogger<MongoDbInstaller>();
        }

        protected readonly IMongoFactory Factory;

        protected readonly ILogger Logger;
        protected readonly ISettingProvider Settings;


        [JsonIgnore] private MongoOption _mongoOption;

        public string AssemblyDbInstaller => GetType().Assembly.GetName().FullName;

        public IMongoDbInstaller SetDatabaseTo(MongoOption option)
        {
            _mongoOption = option;
            Factory.SetDatabaseTo(option);
            return this;
        }

        public void CreateTables()
            => AsyncHelper.RunSync(() => CreateTablesAsync());

        public Task CreateTablesAsync(CancellationToken token = default)
            => _mongoOption.EnableMigration == false ? Task.CompletedTask : CreateTablesAsync(_mongoOption, token);

        public void CreateIndexes()
            => AsyncHelper.RunSync(() => CreateIndexesAsync());

        public Task CreateIndexesAsync(CancellationToken token = default)
            => _mongoOption.EnableMigration == false ? Task.CompletedTask : CreateIndexesAsync(_mongoOption, token);

        public void Seed()
            => AsyncHelper.RunSync(() => SeedAsync());

        public Task SeedAsync(CancellationToken token = default)
            => _mongoOption.EnableMigration == false ? Task.CompletedTask : SeedAsync(_mongoOption, token);

        public void Execute()
            => AsyncHelper.RunSync(() => ExecuteAsync());

        public Task ExecuteAsync(CancellationToken token = default)
            => MigrationAsync(_mongoOption, token);

        protected virtual Task CreateTablesAsync(MongoOption option, CancellationToken token = default)
        {
            if (!option.EnableMigration)
                return Task.CompletedTask;
            Logger.LogTrace("Migration database is starting");

            try
            {
                var collectionOptions = new CreateCollectionOptions();
                var collation = new Collation(option.Localization);
                collectionOptions.Collation = collation;

                var assembly = Assembly.GetEntryAssembly();
                var assemblyType = option.DocumentTypesAssembly;

                if (assemblyType.IsNotEmpty())
                    assembly = Assembly.Load(assemblyType);

                if (assembly is null)
                {
                    Logger.LogError(
                        "MongoDbInstaller can't create tables, Invalid migration assembly {DocumentTypesAssembly}",
                        option.DocumentTypesAssembly);
                    return Task.CompletedTask;
                }

                var types = assembly.GetTypes().Where(x =>
                    !x.IsAbstract &&
                    x.IsClass &&
                    x.GetCustomAttribute<MigrationIgnoreAttribute>() != null &&
                    x.IsAssignableTo(typeof(IAggregateRoot)));

                var dbContext = Factory.DbContext;

                foreach (var item in types)
                    dbContext.CreateDropCollection(option, item.GetTableName());

                Logger.LogTrace("Migration database {DatabaseName} is completed", option.Database);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Logger.LogError("{Message}", ex.Message);
                throw new MongoDbException(ex.Message);
            }
        }

        protected virtual Task CreateIndexesAsync(MongoOption options, CancellationToken token = default)
            => Task.CompletedTask;

        protected virtual Task SeedAsync(MongoOption options, CancellationToken token = default)
            => Task.CompletedTask;

        protected virtual async Task MigrationAsync(MongoOption option, CancellationToken token = default)
        {
            try
            {
                await _asyncLock.WaitAsync(token);
                if (option.EnableMigration == false) return;

                await CreateTablesAsync(token);
                await CreateIndexesAsync(token);

                if (option.EnableSeed)
                    await SeedAsync(token);

                if (option.MigrationStrategy == DatabaseMigrationStrategy.CreateDropOnce)
                    Settings.DisableMigrationFor(option);
            }
            catch (Exception e)
            {
                if (option.ThrowIfMigrationFailed)
                    throw new MongoDbException(e.Message, e);
            }
            finally
            {
                _asyncLock.Release();
            }
        }
    }
}