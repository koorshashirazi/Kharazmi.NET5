#region

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Kharazmi.Common.Domain;
using Kharazmi.Data;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

#endregion

namespace Kharazmi.Localization
{
    public interface IMongoRepository<TAggregateRoot> : IRepository<TAggregateRoot, string>
        where TAggregateRoot : class, IAggregateRoot<string>
    {
        #region Query

        /// <summary>
        /// Get collection by filter definitions
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        IList<TAggregateRoot> GetBy(FilterDefinition<TAggregateRoot> query);

        /// <summary>
        /// Get collection by filter expression
        /// </summary>
        /// <param name="query"></param>
        /// <returns>IMongoQueryable</returns>
        IMongoQueryable<TAggregateRoot> GetByQuery(Expression<Func<TAggregateRoot, bool>> query);

        /// <summary>
        /// Get collection by filter expression
        /// </summary>
        /// <param name="query"></param>
        /// <returns>IFindFluent</returns>
        IFindFluent<TAggregateRoot, TAggregateRoot> GetFluent(Expression<Func<TAggregateRoot, bool>> query);

        #endregion

        #region Command

        /// <summary>
        /// Replace matches entity, insert the document if it doesn't already exist.
        /// </summary>
        /// <param name="findQuery"></param>
        /// <param name="updateQuery"></param>
        /// <param name="useTransaction">Replace entity  with transaction session scope</param>
        void FindAndReplace(
            Expression<Func<TAggregateRoot, bool>> findQuery,
            UpdateDefinition<TAggregateRoot> updateQuery,
            bool useTransaction = false);

        /// <summary>
        /// Async Replace matches entity, insert the document if it doesn't already exist.
        /// </summary>
        /// <param name="findQuery"></param>
        /// <param name="updateQuery"></param>
        /// <param name="useTransaction">Async Replace entity with transaction session scope</param>
        Task FindAndReplaceAsync(
            Expression<Func<TAggregateRoot, bool>> findQuery,
            UpdateDefinition<TAggregateRoot> updateQuery,
            bool useTransaction = false);


        /// <summary>
        /// Find one and update
        /// </summary>
        /// <param name="findQuery"></param>
        /// <param name="updateQuery"></param>
        /// <param name="useTransaction">Find and updateAsync with transaction session scope</param>
        void FindAndUpdate(Expression<Func<TAggregateRoot, bool>> findQuery,
            UpdateDefinition<TAggregateRoot> updateQuery, bool useTransaction = false);

        /// <summary>
        /// Find one and update
        /// </summary>
        /// <param name="findQuery"></param>
        /// <param name="updateQuery"></param>
        /// <param name="useTransaction">Find and updateAsync with transaction session scope</param>
        /// <returns></returns>
        Task FindAndUpdateAsync(Expression<Func<TAggregateRoot, bool>> findQuery,
            UpdateDefinition<TAggregateRoot> updateQuery, bool useTransaction = false);

        #endregion
    }
}