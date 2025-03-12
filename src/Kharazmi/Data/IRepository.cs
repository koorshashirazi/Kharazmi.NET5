using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Kharazmi.Collections;
using Kharazmi.Common.Domain;
using Kharazmi.Common.ValueObjects;
using Kharazmi.Dependency;

namespace Kharazmi.Data
{
    public interface IRepository<TAggregateRoot, TKey> : IMustBeInstance
        where TAggregateRoot : class, IAggregateRoot<TKey>
        where TKey : IEquatable<TKey>, IComparable
    {
        #region Query

        /// <summary>
        /// Get entity by id
        /// </summary>
        /// <param name="id">Identifier</param>
        /// <returns>Entity</returns>
        Maybe<TAggregateRoot> FindById(IIdentity<string> id);

        /// <summary>
        /// Get async entity by id 
        /// </summary>
        /// <param name="id">Identifier</param>
        /// <returns>Entity</returns>
        Task<Maybe<TAggregateRoot>> FindByIdAsync(IIdentity<string> id);

        /// <summary>
        /// find entity
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns>Entity</returns>
        Maybe<TAggregateRoot> FindBy(Expression<Func<TAggregateRoot, bool>> predicate);

        /// <summary>
        /// fin async entity
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns>Entity</returns>
        Task<Maybe<TAggregateRoot>> FindByAsync(Expression<Func<TAggregateRoot, bool>> predicate);

        /// <summary>
        /// Determines whether a list contains any elements
        /// </summary>
        /// <returns></returns>
        bool Any();

        /// <summary>
        /// Determines whether any element of a list satisfies a condition.
        /// </summary>
        /// <param name="predication"></param>
        /// <returns></returns>
        bool Any(Expression<Func<TAggregateRoot, bool>> predication);

        /// <summary>
        /// Async determines whether a list contains any elements
        /// </summary>
        /// <returns></returns>
        Task<bool> AnyAsync();

        /// <summary>
        /// Async determines whether any element of a list satisfies a condition.
        /// </summary>
        /// <param name="predication"></param>
        /// <returns></returns>
        Task<bool> AnyAsync(Expression<Func<TAggregateRoot, bool>> predication);

        /// <summary>
        /// Returns the number of elements in the specified sequence.
        /// </summary>
        /// <returns></returns>
        int Count();

        /// <summary>
        /// Returns the number of elements in the specified sequence that satisfies a condition.
        /// </summary>
        /// <param name="predication"></param>
        /// <returns></returns>
        int Count(Expression<Func<TAggregateRoot, bool>> predication);

        /// <summary>
        /// Async returns the number of elements in the specified sequence
        /// </summary>
        /// <returns></returns>
        Task<int> CountAsync();

        /// <summary>
        /// Async returns the number of elements in the specified sequence that satisfies a condition.
        /// </summary>
        /// <param name="predication"></param>
        /// <returns></returns>
        Task<int> CountAsync(Expression<Func<TAggregateRoot, bool>> predication);

        /// <summary>
        /// Async returns the number of elements in the specified sequence
        /// </summary>
        /// <returns></returns>
        long LongCount();

        /// <summary>
        /// Async returns the number of elements in the specified sequence
        /// </summary>
        /// <returns></returns>
        Task<long> LongCountAsync();

        /// <summary>
        /// Async returns the number of elements in the specified sequence that satisfies a condition.
        /// </summary>
        /// <param name="predication"></param>
        /// <returns></returns>
        Task<long> LongCountAsync(Expression<Func<TAggregateRoot, bool>> predication);

        /// <summary>
        /// Get collection by filter expression
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        List<TAggregateRoot> GetBy(Expression<Func<TAggregateRoot, bool>> query);


        /// <summary>
        /// et collection by filter expression
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        Task<List<TAggregateRoot>> GetByAsync(Expression<Func<TAggregateRoot, bool>> query);

        /// <summary>
        /// Return PagedList as paged
        /// </summary>
        /// <param name="predication"></param>
        /// <param name="orderBy"></param>
        /// <param name="pageSize">default is 10</param>
        /// <param name="page">Default is 1</param>
        /// <param name="isAsc">Default is true</param>
        /// <typeparam name="TOrderKey"></typeparam>
        /// <returns></returns>
        IPagedList<TAggregateRoot> PageBy<TOrderKey>(
            Expression<Func<TAggregateRoot, TOrderKey>>? orderBy = null,
            Expression<Func<TAggregateRoot, bool>>? predication = null,
            int pageSize = 10, int page = 1, bool isAsc = true);

        /// <summary>
        /// Return PagedList as paged
        /// </summary>
        /// <param name="predication"></param>
        /// <param name="orderBy"></param>
        /// <param name="pageSize">default is 10</param>
        /// <param name="page">Default is 1</param>
        /// <param name="isAsc">Default is true</param>
        /// <typeparam name="TOrderKey"></typeparam>
        /// <returns></returns>
        Task<IPagedList<TAggregateRoot>> PageByAsync<TOrderKey>(
            Expression<Func<TAggregateRoot, TOrderKey>>? orderBy = null,
            Expression<Func<TAggregateRoot, bool>>? predication = null,
            int pageSize = 10, int page = 1, bool isAsc = true);

        #endregion

        #region Commands

        /// <summary>
        /// Insert entity
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <param name="useTransaction">Insert entity with transaction session scope</param>
        void Add(TAggregateRoot entity, bool useTransaction = false);

        /// <summary>
        /// Async Insert entity
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <param name="useTransaction">Async Insert entity with transaction session scope</param>
        Task AddAsync(TAggregateRoot entity, bool useTransaction = false);

        /// <summary>
        /// Insert entities
        /// </summary>
        /// <param name="entities">Entities</param>
        /// <param name="useTransaction">Insert entities with transaction session scope</param>
        void Add(IEnumerable<TAggregateRoot> entities, bool useTransaction = false);

        /// <summary>
        /// Async Insert entities
        /// </summary>
        /// <param name="entities">Entities</param>
        /// <param name="useTransaction"> Async Insert entities with transaction session scope</param>
        Task AddAsync(IEnumerable<TAggregateRoot> entities, bool useTransaction = false);


        /// <summary>
        /// Update entity
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <param name="useTransaction">Update entity  with transaction session scope</param>
        void Update(TAggregateRoot entity, bool useTransaction = false);


        /// <summary>
        /// Async Update entity 
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <param name="useTransaction">Async Update entity with transaction session scope</param>
        Task UpdateAsync(TAggregateRoot entity, bool useTransaction = false);

        /// <summary>
        /// Update entities 
        /// </summary>
        /// <param name="entities">Entities</param>
        /// <param name="useTransaction">Update entities with transaction session scope</param>
        void Update(IEnumerable<TAggregateRoot> entities, bool useTransaction = false);

        /// <summary>
        /// Async Update entities
        /// </summary>
        /// <param name="entities">Entities</param>
        /// <param name="useTransaction">Async Update entities with transaction session scope</param>
        Task UpdateAsync(IEnumerable<TAggregateRoot> entities, bool useTransaction = false);

        /// <summary>
        /// Replace entity, insert the document if it doesn't already exist.
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <param name="useTransaction">Replace entity  with transaction session scope</param>
        void AddOrUpdate(TAggregateRoot entity, bool useTransaction = false);

        /// <summary>
        /// Async Replace entity, insert the document if it doesn't already exist.
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <param name="useTransaction">Async Replace entity with transaction session scope</param>
        Task AddOrUpdateAsync(TAggregateRoot entity, bool useTransaction = false);

        /// <summary>
        /// Replace entities, insert the document if it doesn't already exist.
        /// </summary>
        /// <param name="entities">Entities</param>
        /// <param name="useTransaction">Replace entities with transaction session scope</param>
        void AddOrUpdate(IEnumerable<TAggregateRoot> entities, bool useTransaction = false);

        /// <summary>
        /// Async Replace entities, insert the document if it doesn't already exist.
        /// </summary>
        /// <param name="entities">Entities</param>
        /// <param name="useTransaction">Async Replace entities with transaction session scope</param>
        Task AddOrUpdateAsync(IEnumerable<TAggregateRoot> entities, bool useTransaction = false);

        /// <summary>
        /// Delete entity
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <param name="useTransaction">Delete entity with transaction session scope</param>
        void Delete(TAggregateRoot entity, bool useTransaction = false);

        /// <summary>
        /// Async Delete entity
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <param name="useTransaction">Async Delete entity with transaction session scope</param>
        Task DeleteAsync(TAggregateRoot entity, bool useTransaction = false);


        /// <summary>
        /// Delete entities 
        /// </summary>
        /// <param name="entities">Entities</param>
        /// <param name="useTransaction">Delete entities with transaction session scope</param>
        void Delete(IEnumerable<TAggregateRoot> entities, bool useTransaction = false);


        /// <summary>
        /// Async Delete entities
        /// </summary>
        /// <param name="entities">Entities</param>
        /// <param name="useTransaction">Async Delete entities with transaction session scope</param>
        Task DeleteAsync(IEnumerable<TAggregateRoot> entities, bool useTransaction = false);

        void FindAndDelete(Expression<Func<TAggregateRoot, bool>> findQuery, bool useTransaction = false);

        Task FindAndDeleteAsync(Expression<Func<TAggregateRoot, bool>> findQuery, bool useTransaction = false);

        #endregion
    }
}