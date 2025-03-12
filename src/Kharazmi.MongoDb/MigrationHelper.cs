#region

using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kharazmi.Common.Domain;
using Kharazmi.Configuration;
using Kharazmi.Constants;
using Kharazmi.Extensions;
using Kharazmi.Options.Mongo;
using Kharazmi.Threading;
using MongoDB.Driver;

#endregion

namespace Kharazmi.Localization
{
    public static class MigrationHelper
    {
        public static void CreateDropCollection(this IMongoDbContext context,
            MongoOption option, string collectionName)
        {
            if (option.EnableMigration == false) return;

            switch (option.MigrationStrategy)
            {
                case DatabaseMigrationStrategy.Ignore:
                    return;
                case DatabaseMigrationStrategy.CreateDropOnce:
                    AsyncHelper.RunSync(async () => await context.DropCollectionAsync(collectionName));

                    AsyncHelper.RunSync(async () =>
                        await context.CreateCollectionAsync(collectionName, new CreateCollectionOptions()));


                    return;
                case DatabaseMigrationStrategy.AlwaysCreateDrop:
                    AsyncHelper.RunSync(async () => await context.DropCollectionAsync(collectionName));

                    AsyncHelper.RunSync(async () =>
                        await context.CreateCollectionAsync(collectionName, new CreateCollectionOptions()));
                    return;
                case DatabaseMigrationStrategy.CreateIfNotExist:
                    AsyncHelper.RunSync(async () =>
                    {
                        if (!context.Exists(collectionName))
                            await context.CreateCollectionAsync(collectionName, new CreateCollectionOptions());
                    });
                    return;
                case DatabaseMigrationStrategy.DropIfExist:
                    AsyncHelper.RunSync(async () =>
                    {
                        if (context.Exists(collectionName))
                            await context.DropCollectionAsync(collectionName);
                    });
                    return;
            }
        }

        public static void CreateIndex<TDocument>(this IMongoCollection<TDocument> collection,
            MongoOption mongoOption,
            CreateIndexOptions? indexOptions = null,
            bool unique = false,
            params IndexBuilder<TDocument>[] indexBuilders)
            where TDocument : IAggregateRoot
            => AsyncHelper.RunSync(() =>
                CreateIndexAsync(collection, mongoOption, indexOptions, unique, indexBuilders));

        public static async Task CreateIndexAsync<TDocument>(
            this IMongoCollection<TDocument> collection,
            MongoOption mongoOption,
            CreateIndexOptions? indexOptions = null,
            bool unique = false,
            params IndexBuilder<TDocument>[] indexBuilders)
            where TDocument : IAggregateRoot
        {
            var builder = Builders<TDocument>.IndexKeys;
            IndexKeysDefinition<TDocument>? indexKeysAss = null;
            IndexKeysDefinition<TDocument>? indexKeysDes = null;
            IndexKeysDefinition<TDocument>? definition = null;


            var option = indexOptions ?? new CreateIndexOptions();

            void IndexTask()
            {
                var indexKey = new StringBuilder(typeof(TDocument).Name);
                indexKey.Append('_');

                var assExpression = indexBuilders.Where(x => x.IsAscending).Select(x => x.Expression).ToList();
                var desExpression = indexBuilders.Where(x => !x.IsAscending).Select(x => x.Expression).ToList();

                var firstAss = assExpression.FirstOrDefault();
                if (firstAss is not null)
                {
                    indexKey.Append(firstAss.GetPropertyName());
                    indexKey.Append('_');
                    indexKeysAss = builder.Ascending(firstAss);
                    assExpression.RemoveAt(0);

                    foreach (var expression in assExpression)
                    {
                        indexKey.Append(expression.GetPropertyName());
                        indexKey.Append('_');
                        builder.Combine(indexKeysAss, builder.Ascending(expression));
                    }
                }

                var firstDes = desExpression.FirstOrDefault();
                if (firstDes is not null)
                {
                    indexKey.Append(firstDes.GetPropertyName());
                    indexKey.Append('_');
                    indexKeysDes = builder.Ascending(firstDes);
                    desExpression.RemoveAt(0);
                    foreach (var expression in desExpression)
                    {
                        indexKey.Append(expression.GetPropertyName());
                        indexKey.Append('_');
                        builder.Combine(indexKeysDes, builder.Descending(expression));
                    }
                }

                if (indexKeysAss is not null)
                    definition = indexKeysAss;

                if (indexKeysDes is not null)
                    definition = indexKeysDes;

                if (indexKeysAss is not null && indexKeysDes is not null)
                    definition = builder.Combine(indexKeysAss, indexKeysDes);

                if (definition is null)
                    return;

                option.Name = indexKey.ToString();
                option.Unique = unique;
                option.Background = true;
            }

            IndexTask();

            switch (mongoOption.MigrationStrategy)
            {
                case DatabaseMigrationStrategy.Ignore:
                    return;
                case DatabaseMigrationStrategy.CreateDropOnce:
                    await collection.Indexes.DropOneAsync(option.Name);
                    await collection.Indexes.CreateOneAsync(new CreateIndexModel<TDocument>(definition, option));
                    return;

                case DatabaseMigrationStrategy.AlwaysCreateDrop:
                    await collection.Indexes.DropOneAsync(option.Name);
                    await collection.Indexes.CreateOneAsync(new CreateIndexModel<TDocument>(definition, option));
                    return;
                case DatabaseMigrationStrategy.CreateIfNotExist:
                    var list = await collection.Indexes.ListAsync();
                    var documents = await list.ToListAsync();

                    bool isFound = false;
                    foreach (var document in documents)
                    {
                        document.TryGetElement("name", out var value);

                        if (option.Name == value.Value)
                            isFound = true;
                    }

                    if (isFound == false)
                        await collection.Indexes.CreateOneAsync(new CreateIndexModel<TDocument>(definition, option));

                    return;
                case DatabaseMigrationStrategy.DropIfExist:
                    var list1 = await collection.Indexes.ListAsync();
                    var documents1 = await list1.ToListAsync();

                    foreach (var document in documents1)
                    {
                        document.TryGetElement("name", out var value);
                        if (value.Value == option.Name)
                            await collection.Indexes.DropOneAsync(option.Name);
                    }

                    return;
            }
        }

        public static void CreateIndex<TDocument>(
            this IMongoDbContext dbContext,
            MongoOption mongoOption,
            CreateIndexOptions? indexOptions = null,
            bool unique = false,
            params IndexBuilder<TDocument>[] indexBuilders)
            where TDocument : IAggregateRoot
            => AsyncHelper.RunSync(() => CreateIndexAsync(dbContext, mongoOption, indexOptions, unique, indexBuilders));

        public static Task CreateIndexAsync<TDocument>(
            this IMongoDbContext dbContext,
            MongoOption mongoOption,
            CreateIndexOptions? indexOptions = null,
            bool unique = false,
            params IndexBuilder<TDocument>[] indexBuilders)
            where TDocument : IAggregateRoot
        {
            var collection = dbContext.GetCollection<TDocument>();
            if (collection is null)
                throw new MongoDbException($"Can't not get collection with name {typeof(TDocument).Name}");

            return collection.CreateIndexAsync(mongoOption, indexOptions, unique, indexBuilders);
        }

        public static void DisableMigrationFor(this ISettingProvider settings, MongoOption option)
        {
            var options = settings.Get<MongoOptions>();
            option.EnableMigration = false;
            options.UpdateChildOption(option);
            settings.UpdateOption(options);
            settings.SaveChanges();
        }
    }
}