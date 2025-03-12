#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Kharazmi.Collections;
using Kharazmi.Common.Domain;
using Kharazmi.Common.ValueObjects;
using Kharazmi.Extensions;
using Kharazmi.Localization.Extensions;
using Kharazmi.Threading;
using MongoDB.Driver;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Linq;

#endregion

namespace Kharazmi.Localization
{
    // MongoRedisRepository use this
    public class MongoRepository<TAggregateRoot> : IMongoRepository<TAggregateRoot>
        where TAggregateRoot : class, IAggregateRoot<string>
    {
        public MongoRepository(IMongoDbContext dbContext)
        {
            DbContext = dbContext;

            CollectionName = typeof(TAggregateRoot).GetTableName();

            var collection = DbContext.GetCollection<TAggregateRoot>(CollectionName);

            DbSet = collection ??
                    throw new MongoDbException($"Can't not get collection with collection name: {CollectionName}");
        }

        protected IMongoDbContext DbContext { get; }

        protected string CollectionName { get; set; }
        protected IMongoCollection<TAggregateRoot> DbSet { get; }

        #region Queries

        protected virtual IMongoQueryable<TAggregateRoot> Table => DbSet.AsQueryable();


        public virtual Maybe<TAggregateRoot> FindById(IIdentity<string> id)
            => AsyncHelper.RunAsSync(() => FindByIdAsync(id));

        public virtual async Task<Maybe<TAggregateRoot>> FindByIdAsync(IIdentity<string> id)
            => await Table.FirstOrDefaultAsync(x => x.Id == id.Value);

        public virtual Maybe<TAggregateRoot> FindBy(Expression<Func<TAggregateRoot, bool>> predicate)
            => AsyncHelper.RunAsSync(() => FindByAsync(predicate));

        public virtual async Task<Maybe<TAggregateRoot>> FindByAsync(Expression<Func<TAggregateRoot, bool>> predicate)
            => await Table.SingleOrDefaultAsync(predicate);

        public virtual bool Any() => AsyncHelper.RunSync(AnyAsync);

        public virtual bool Any(Expression<Func<TAggregateRoot, bool>> predication)
            => AsyncHelper.RunSync(() => AnyAsync(predication));

        public virtual Task<bool> AnyAsync() => Table.AnyAsync();


        public virtual Task<bool> AnyAsync(Expression<Func<TAggregateRoot, bool>> predication)
            => Table.AnyAsync(predication);

        public virtual int Count() => AsyncHelper.RunAsSync(CountAsync);

        public virtual int Count(Expression<Func<TAggregateRoot, bool>> predication)
            => AsyncHelper.RunSync(() => CountAsync(predication));

        public virtual Task<int> CountAsync() => Table.CountAsync();

        public virtual Task<int> CountAsync(Expression<Func<TAggregateRoot, bool>> predication) =>
            Table.CountAsync(predication);

        public long LongCount()
            => AsyncHelper.RunSync(LongCountAsync);

        public async Task<long> LongCountAsync()
        {
            var count = await Table.CountAsync();
            return Convert.ToInt64(count);
        }

        public async Task<long> LongCountAsync(Expression<Func<TAggregateRoot, bool>> predication)
        {
            var count = await Table.CountAsync(predication);
            return Convert.ToInt64(count);
        }


        public virtual IList<TAggregateRoot> GetBy(FilterDefinition<TAggregateRoot> query)
            => DbSet.Find(query).ToList();

        public virtual List<TAggregateRoot> GetBy(Expression<Func<TAggregateRoot, bool>> query)
            => AsyncHelper.RunSync(() => GetByAsync(query));

        public virtual IMongoQueryable<TAggregateRoot> GetByQuery(Expression<Func<TAggregateRoot, bool>> query)
            => Table.Where(query);

        public virtual IFindFluent<TAggregateRoot, TAggregateRoot> GetFluent(
            Expression<Func<TAggregateRoot, bool>> query)
            => DbSet.Find(query);

        public virtual Task<List<TAggregateRoot>> GetByAsync(Expression<Func<TAggregateRoot, bool>> query)
            => Table.Where(query).ToListAsync();

        public virtual IPagedList<TAggregateRoot> PageBy<TOrderKey>(
            Expression<Func<TAggregateRoot, TOrderKey>>? orderBy,
            Expression<Func<TAggregateRoot, bool>>? predication = null, int pageSize = 10, int page = 1,
            bool isAsc = true)
            => AsyncHelper.RunAsSync(() => PageByAsync(orderBy, predication, pageSize, page, isAsc));

        public virtual Task<IPagedList<TAggregateRoot>> PageByAsync<TOrderKey>(
            Expression<Func<TAggregateRoot, TOrderKey>>? orderBy = null,
            Expression<Func<TAggregateRoot, bool>>? predication = null,
            int pageSize = 10, int page = 1, bool isAsc = true)
        {
            var filter = Table;
            if (predication != null)
                filter = filter.Where(predication);

            var query = filter;

            if (orderBy != null)
                query = isAsc ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);

            return query.PageByAsync(page, pageSize);
        }

        #endregion


        #region Commands

        public virtual void Add(TAggregateRoot entity, bool useTransaction = false)
            => AsyncHelper.RunAsSync(() => AddAsync(entity, useTransaction));

        public virtual Task AddAsync(TAggregateRoot entity, bool useTransaction = false)
        {
            Ensure.IsNotNull(entity, nameof(entity));
            return useTransaction
                ? AddTransactionAsync(entity)
                : DbContext.AddCommandAsync(GetInsertKey(entity), token =>
                {
                    try
                    {
                        return DbSet.InsertOneAsync(entity, cancellationToken: token);
                    }
                    catch
                    {
                        return DbSet.ReplaceOneAsync(x => x.Id == entity.Id, entity, new ReplaceOptions
                        {
                            IsUpsert = true
                        }, token);
                    }
                });
        }

        protected virtual Task AddTransactionAsync(TAggregateRoot entity)
        {
            Ensure.IsNotNull(entity, nameof(entity));
            return DbContext.AddTransactionCommandAsync(GetInsertKey(entity), (session, token) =>
            {
                try
                {
                    return DbSet.InsertOneAsync(session, entity, cancellationToken: token);
                }
                catch
                {
                    return DbSet.ReplaceOneAsync(session, x => x.Id == entity.Id, entity, new ReplaceOptions
                    {
                        IsUpsert = true
                    }, token);
                }
            });
        }

        public virtual void Add(IEnumerable<TAggregateRoot> entities, bool useTransaction = false)
            => AsyncHelper.RunAsSync(() => AddAsync(entities, useTransaction));


        public virtual Task AddAsync(IEnumerable<TAggregateRoot> entities, bool useTransaction = false)
        {
            if (useTransaction)
                return AddTransactionAsync(entities);

            var list = entities.ToList();

            if (list.Count <= 0) return Task.CompletedTask;

            var key = list.Average(x => x.GetHashCode());

            return DbContext.AddCommandAsync($"InsertMany-{key}",
                token => DbSet.InsertManyAsync(list, cancellationToken: token));
        }

        protected virtual Task AddTransactionAsync(IEnumerable<TAggregateRoot> entities)
        {
            var list = entities.ToList();

            if (list.Count <= 0) return Task.CompletedTask;

            var key = list.Average(x => x.GetHashCode());

            return DbContext.AddTransactionCommandAsync($"InsertMany-{key}", (session, token) =>
                DbSet.InsertManyAsync(session, list, new InsertManyOptions {IsOrdered = true}, token));
        }


        public virtual void Update(TAggregateRoot entity, bool useTransaction = false)
            => AsyncHelper.RunAsSync(() => UpdateAsync(entity, useTransaction));

        public virtual Task UpdateAsync(TAggregateRoot entity, bool useTransaction = false)
        {
            Ensure.IsNotNull(entity, nameof(entity));
            return useTransaction
                ? UpdateTransactionAsync(entity)
                : DbContext.AddCommandAsync(GetUpdateKey(entity), token =>
                    DbSet.ReplaceOneAsync(x => x.Id == entity.Id, entity,
                        new ReplaceOptions {IsUpsert = false}, token));
        }

        public virtual void Update(IEnumerable<TAggregateRoot> entities, bool useTransaction = false)
            => AsyncHelper.RunAsSync(() => UpdateAsync(entities, useTransaction));


        public virtual Task UpdateAsync(IEnumerable<TAggregateRoot> entities, bool useTransaction = false)
        {
            if (useTransaction)
                return UpdateTransactionAsync(entities);

            var tasks = entities.Select(entity => UpdateAsync(entity));
            return Task.WhenAll(tasks);
        }

        protected virtual Task UpdateTransactionAsync(TAggregateRoot entity)
        {
            return DbContext.AddTransactionCommandAsync(GetUpdateKey(entity), (session, token) =>
                DbSet.ReplaceOneAsync(session,
                    x => x.Id == entity.Id, entity,
                    new ReplaceOptions {IsUpsert = false}, token));
        }

        protected virtual Task UpdateTransactionAsync(IEnumerable<TAggregateRoot> entities)
        {
            var tasks = entities.Select(UpdateTransactionAsync);
            return Task.WhenAll(tasks);
        }


        public void AddOrUpdate(TAggregateRoot entity, bool useTransaction = false)
            => AsyncHelper.RunSync(() => AddOrUpdateAsync(entity, useTransaction));

        public Task AddOrUpdateAsync(TAggregateRoot entity, bool useTransaction = false)
        {
            Ensure.IsNotNull(entity, nameof(entity));
            return useTransaction
                ? AddOrUpdateTransactionAsync(entity)
                : DbContext.AddCommandAsync(GetUpdateKey(entity), token =>
                    DbSet.ReplaceOneAsync(x => x.Id == entity.Id, entity,
                        new ReplaceOptions {IsUpsert = true}, token));
        }

        public void AddOrUpdate(IEnumerable<TAggregateRoot> entities, bool useTransaction = false)
            => AsyncHelper.RunSync(() => AddOrUpdateAsync(entities, useTransaction));

        public Task AddOrUpdateAsync(IEnumerable<TAggregateRoot> entities, bool useTransaction = false)
        {
            if (useTransaction)
                return AddOrUpdateTransactionAsync(entities);

            var tasks = entities.Select(entity => AddOrUpdateAsync(entity));
            return Task.WhenAll(tasks);
        }

        protected virtual Task AddOrUpdateTransactionAsync(TAggregateRoot entity)
        {
            return DbContext.AddTransactionCommandAsync(GetUpdateKey(entity), (session, token) =>
                DbSet.ReplaceOneAsync(session, x => x.Id == entity.Id, entity,
                    new ReplaceOptions {IsUpsert = true}, token));
        }

        protected virtual Task AddOrUpdateTransactionAsync(IEnumerable<TAggregateRoot> entities)
        {
            var tasks = entities.Select(AddOrUpdateTransactionAsync);
            return Task.WhenAll(tasks);
        }

        public void FindAndReplace(Expression<Func<TAggregateRoot, bool>> findQuery,
            UpdateDefinition<TAggregateRoot> updateQuery,
            bool useTransaction = false)
            => AsyncHelper.RunSync(() => FindAndReplaceAsync(findQuery, updateQuery, useTransaction));

        public Task FindAndReplaceAsync(
            Expression<Func<TAggregateRoot, bool>> findQuery,
            UpdateDefinition<TAggregateRoot> updateQuery,
            bool useTransaction = false)
        {
            return useTransaction
                ? FindAndReplaceTransactionAsync(findQuery, updateQuery)
                : DbContext.AddCommandAsync(Guid.NewGuid().ToString("N"), token =>
                    DbSet.UpdateOneAsync(findQuery, updateQuery, new UpdateOptions {IsUpsert = true}, token));
        }


        protected virtual Task FindAndReplaceTransactionAsync(Expression<Func<TAggregateRoot, bool>> findQuery,
            UpdateDefinition<TAggregateRoot> updateQuery)
        {
            return DbContext.AddTransactionCommandAsync(Guid.NewGuid().ToString("N"), (session, token) =>
                DbSet.UpdateManyAsync(session, findQuery, updateQuery, new UpdateOptions {IsUpsert = true}, token));
        }

        public virtual void Delete(TAggregateRoot entity, bool useTransaction = false)
            => AsyncHelper.RunAsSync(() => DeleteAsync(entity, useTransaction));

        public virtual Task DeleteAsync(TAggregateRoot entity, bool useTransaction = false)
        {
            Ensure.IsNotNull(entity, nameof(entity));
            return useTransaction
                ? DeleteTransactionAsync(entity)
                : DbContext.AddCommandAsync(GetDeleteKey(entity), token =>
                    DbSet.DeleteOneAsync(x => x.Id == entity.Id, token));
        }

        protected virtual Task DeleteTransactionAsync(TAggregateRoot entity)
        {
            Ensure.IsNotNull(entity, nameof(entity));
            return DbContext.AddTransactionCommandAsync(GetDeleteKey(entity), (session, token) =>
                DbSet.DeleteOneAsync(session, x => x.Id == entity.Id, new DeleteOptions(),
                    token));
        }

        public virtual void Delete(IEnumerable<TAggregateRoot> entities, bool useTransaction = false)
            => AsyncHelper.RunSync(() => DeleteAsync(entities, useTransaction));

        public virtual Task DeleteAsync(IEnumerable<TAggregateRoot> entities, bool useTransaction = false)
        {
            if (useTransaction)
                return DeleteTransactionAsync(entities);

            var tasks = entities.Select(entity => DeleteAsync(entity));

            return Task.WhenAll(tasks);
        }

        protected virtual Task DeleteTransactionAsync(IEnumerable<TAggregateRoot> entities)
        {
            var tasks = entities.Select(DeleteTransactionAsync);

            return Task.WhenAll(tasks);
        }

        #region Query&Command

        public virtual void FindAndUpdate(Expression<Func<TAggregateRoot, bool>> findQuery,
            UpdateDefinition<TAggregateRoot> updateQuery, bool useTransaction = false)
            => AsyncHelper.RunAsSync(() => FindAndUpdateAsync(findQuery, updateQuery, useTransaction));

        public virtual async Task FindAndUpdateAsync(Expression<Func<TAggregateRoot, bool>> findQuery,
            UpdateDefinition<TAggregateRoot> updateQuery, bool useTransaction = false)
        {
            Ensure.IsNotNull(findQuery, nameof(findQuery));
            if (useTransaction)
            {
                await FindAndUpdateTransactionAsync(findQuery, updateQuery);
                return;
            }

            var entity = await FindByAsync(findQuery);
            if (!entity.HasValue)
                return;

            await DbContext.AddCommandAsync(GetUpdateKey(entity.Value), token => DbSet.FindOneAndUpdateAsync(findQuery,
                updateQuery, new FindOneAndUpdateOptions<TAggregateRoot>
                {
                    IsUpsert = false
                }, token)).ConfigureAwait(false);
        }

        private async Task FindAndUpdateTransactionAsync(Expression<Func<TAggregateRoot, bool>> findQuery,
            UpdateDefinition<TAggregateRoot> updateQuery)
        {
            Ensure.IsNotNull(findQuery, nameof(findQuery));
            var entity = await FindByAsync(findQuery);
            if (!entity.HasValue)
                return;

            await DbContext.AddTransactionCommandAsync(GetUpdateKey(entity.Value), async (session, token) =>
                await DbSet.FindOneAndUpdateAsync(session, findQuery,
                    updateQuery, new FindOneAndUpdateOptions<TAggregateRoot>
                    {
                        IsUpsert = false
                    }, token)).ConfigureAwait(false);
        }


        public virtual void FindAndDelete(Expression<Func<TAggregateRoot, bool>> findQuery, bool useTransaction = false)
            => AsyncHelper.RunSync(() => FindAndDeleteAsync(findQuery, useTransaction));

        public virtual async Task FindAndDeleteAsync(Expression<Func<TAggregateRoot, bool>> findQuery,
            bool useTransaction = false)
        {
            if (useTransaction)
            {
                await FindAndDeleteTransactionAsync(findQuery);
                return;
            }

            Ensure.IsNotNull(findQuery, nameof(findQuery));

            var entity = await FindByAsync(findQuery);
            if (!entity.HasValue)
                return;
            await DeleteAsync(entity.Value).ConfigureAwait(false);
        }

        private async Task FindAndDeleteTransactionAsync(Expression<Func<TAggregateRoot, bool>> findQuery)
        {
            Ensure.IsNotNull(findQuery, nameof(findQuery));
            var entity = await FindByAsync(findQuery);
            if (!entity.HasValue)
                return;
            await DeleteTransactionAsync(entity.Value);
        }

        #endregion

        #endregion

        private static string GetInsertKey(TAggregateRoot? entity)
            => entity is null ? $"Insert_{Guid.NewGuid():N}" : $"Insert_{entity.GetType().Name}:{entity.GetHashCode()}";

        private static string GetDeleteKey(TAggregateRoot? entity)
            => entity is null ? $"Delete_{Guid.NewGuid():N}" : $"Delete_{entity.GetType().Name}:{entity.GetHashCode()}";

        private static string GetUpdateKey(TAggregateRoot? entity)
            => entity is null ? $"Update_{Guid.NewGuid():N}" : $"Update_{entity.GetType().Name}:{entity.GetHashCode()}";
    }
}