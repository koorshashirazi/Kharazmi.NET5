using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Kharazmi.Collections;
using Kharazmi.Common.Domain;
using Kharazmi.Common.ValueObjects;
using Kharazmi.Dependency;
using Kharazmi.Localization.Extensions;
using Kharazmi.Threading;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Kharazmi.Localization.Default
{
    public class NullMongoRepository<TAggregateRoot> : IMongoRepository<TAggregateRoot>, INullInstance
        where TAggregateRoot : class, IAggregateRoot<string>
    {
        public IMongoDbContext DbContext { get; }
        public string CollectionName { get; }
        private IMongoCollection<TAggregateRoot> DbSet { get; }

        public NullMongoRepository()
        {
            DbContext = new NullMongoDbContext();
            CollectionName = typeof(TAggregateRoot).Name;
            DbSet = DbContext.GetCollection<TAggregateRoot>(CollectionName)!;
        }


        #region Queries

        public Maybe<TAggregateRoot> FindById(IIdentity<string> id)
            => AsyncHelper.RunAsSync(() => FindByIdAsync(id));

        public Task<Maybe<TAggregateRoot>> FindByIdAsync(IIdentity<string> id)
            => Task.FromResult(Maybe<TAggregateRoot>.None);

        public Maybe<TAggregateRoot> FindBy(Expression<Func<TAggregateRoot, bool>> predicate)
            => AsyncHelper.RunAsSync(() => FindByAsync(predicate));

        public Task<Maybe<TAggregateRoot>> FindByAsync(Expression<Func<TAggregateRoot, bool>> predicate)
            => Task.FromResult(Maybe<TAggregateRoot>.None);

        public bool Any()
            => AsyncHelper.RunSync(AnyAsync);

        public bool Any(Expression<Func<TAggregateRoot, bool>> predication)
            => AsyncHelper.RunSync(() => AnyAsync(predication));

        public Task<bool> AnyAsync()
            => Task.FromResult(false);

        public Task<bool> AnyAsync(Expression<Func<TAggregateRoot, bool>> predication)
            => Task.FromResult(false);

        public int Count()
            => AsyncHelper.RunAsSync(CountAsync);

        public int Count(Expression<Func<TAggregateRoot, bool>> predication)
            => AsyncHelper.RunSync(() => CountAsync(predication));

        public Task<int> CountAsync()
            => Task.FromResult(0);

        public Task<int> CountAsync(Expression<Func<TAggregateRoot, bool>> predication)
            => Task.FromResult(0);

        public long LongCount()
            => 0;

        public Task<long> LongCountAsync()
            => Task.FromResult<long>(0);

        public Task<long> LongCountAsync(Expression<Func<TAggregateRoot, bool>> predication)
            => Task.FromResult<long>(0);


        public IList<TAggregateRoot> GetBy(FilterDefinition<TAggregateRoot> query)
            => new List<TAggregateRoot>();

        public List<TAggregateRoot> GetBy(Expression<Func<TAggregateRoot, bool>> query)
            => AsyncHelper.RunSync(() => GetByAsync(query));

        public IMongoQueryable<TAggregateRoot> GetByQuery(Expression<Func<TAggregateRoot, bool>> query)
            => DbSet.AsQueryable();

        public IFindFluent<TAggregateRoot, TAggregateRoot> GetFluent(Expression<Func<TAggregateRoot, bool>> query)
            => DbSet.Find(query);


        public Task FindAndUpdateAsync(Expression<Func<TAggregateRoot, bool>> findQuery,
            UpdateDefinition<TAggregateRoot> updateQuery, bool useTransaction = false)
            => Task.CompletedTask;

        public Task<List<TAggregateRoot>> GetByAsync(Expression<Func<TAggregateRoot, bool>> query)
            => Task.FromResult(new List<TAggregateRoot>());

        public IPagedList<TAggregateRoot> PageBy<TOrderKey>(Expression<Func<TAggregateRoot, TOrderKey>>? orderBy,
            Expression<Func<TAggregateRoot, bool>>? predication = null, int pageSize = 10, int page = 1,
            bool isAsc = true)
            => AsyncHelper.RunAsSync(() => PageByAsync(orderBy, predication, pageSize, page, isAsc));

        public Task<IPagedList<TAggregateRoot>> PageByAsync<TOrderKey>(
            Expression<Func<TAggregateRoot, TOrderKey>>? orderBy = null,
            Expression<Func<TAggregateRoot, bool>>? predication = null,
            int pageSize = 10, int page = 1, bool isAsc = true)
        {
            var filter = DbSet.AsQueryable();
            if (predication != null)
                filter = filter.Where(predication);

            var query = filter;

            if (orderBy != null)
                query = isAsc ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);

            return query.PageByAsync(page, pageSize);
        }

        #endregion


        #region Commands

        public void FindAndReplace(Expression<Func<TAggregateRoot, bool>> findQuery,
            UpdateDefinition<TAggregateRoot> updateQuery, bool useTransaction = false)
        {
        }

        public Task FindAndReplaceAsync(Expression<Func<TAggregateRoot, bool>> findQuery,
            UpdateDefinition<TAggregateRoot> updateQuery, bool useTransaction = false)
            => Task.CompletedTask;

        public void FindAndUpdate(Expression<Func<TAggregateRoot, bool>> findQuery,
            UpdateDefinition<TAggregateRoot> updateQuery, bool useTransaction = false)
        {
        }

        public void Add(TAggregateRoot entity, bool useTransaction = false)
        {
        }

        public Task AddAsync(TAggregateRoot entity, bool useTransaction = false)
            => Task.CompletedTask;

        public void Add(IEnumerable<TAggregateRoot> entities, bool useTransaction = false)
        {
        }

        public Task AddAsync(IEnumerable<TAggregateRoot> entities, bool useTransaction = false)
            => Task.CompletedTask;

        public void Update(TAggregateRoot entity, bool useTransaction = false)
        {
        }

        public Task UpdateAsync(TAggregateRoot entity, bool useTransaction = false)
            => Task.CompletedTask;

        public void Update(IEnumerable<TAggregateRoot> entities, bool useTransaction = false)
        {
        }

        public Task UpdateAsync(IEnumerable<TAggregateRoot> entities, bool useTransaction = false)
            => Task.CompletedTask;

        public void AddOrUpdate(TAggregateRoot entity, bool useTransaction = false)
        {
        }

        public Task AddOrUpdateAsync(TAggregateRoot entity, bool useTransaction = false)
            => Task.CompletedTask;

        public void AddOrUpdate(IEnumerable<TAggregateRoot> entities, bool useTransaction = false)
        {
        }

        public Task AddOrUpdateAsync(IEnumerable<TAggregateRoot> entities, bool useTransaction = false)
            => Task.CompletedTask;

        public void Delete(TAggregateRoot entity, bool useTransaction = false)
        {
        }

        public Task DeleteAsync(TAggregateRoot entity, bool useTransaction = false)
            => Task.CompletedTask;

        public void Delete(IEnumerable<TAggregateRoot> entities, bool useTransaction = false)
        {
        }

        public Task DeleteAsync(IEnumerable<TAggregateRoot> entities, bool useTransaction = false)
            => Task.CompletedTask;

        public void FindAndDelete(Expression<Func<TAggregateRoot, bool>> findQuery, bool useTransaction = false)
        {
        }

        public Task FindAndDeleteAsync(Expression<Func<TAggregateRoot, bool>> findQuery, bool useTransaction = false)
            => Task.CompletedTask;

        #endregion
    }
}