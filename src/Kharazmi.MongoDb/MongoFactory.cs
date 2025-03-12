using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Common.Domain;
using Kharazmi.Dependency;
using Kharazmi.Functional;
using Kharazmi.Localization.Default;
using Kharazmi.Options.Mongo;
using Kharazmi.Threading;
using Microsoft.Extensions.Logging;

namespace Kharazmi.Localization
{
    public class MongoFactory : IMongoFactory, IDisposable
    {
        private readonly object _lock = new();
        private readonly SemaphoreSlim _asyncLock = new(1, 1);
        protected readonly ILoggerFactory LoggerFactory;


        public MongoFactory(ServiceFactory<IMongoDbContext> mongoDbFactory)
        {
            DbContext = mongoDbFactory.Instance();
            LoggerFactory = mongoDbFactory.LoggerFactory;
        }

        public IMongoDbContext DbContext { get; }

        public int GetCommandsCount => DbContext.GetCommandsCount;
        public int GetTransactionCommandsCount => DbContext.GetTransactionCommandsCount;

        public IMongoFactory SetDatabaseTo(MongoOption option)
        {
            DbContext.SetDatabaseTo(option);
            return this;
        }

        public virtual IMongoRepository<TAggregateRoot> Repository<TAggregateRoot>()
            where TAggregateRoot : class, IAggregateRoot<string>
        {
            try
            {
                Monitor.Enter(_lock);
                return new MongoRepository<TAggregateRoot>(DbContext);
            }
            finally
            {
                Monitor.Exit(_lock);
            }
        }

        public virtual IMongoRepository<TAggregateRootCache> Cache<TAggregateRootCache>()
            where TAggregateRootCache : class, IAggregateRootCache<string> =>
            new NullMongoRepository<TAggregateRootCache>();

        public virtual Result SaveChanges()
            => AsyncHelper.RunSync(() => SaveChangesAsync(CancellationToken.None));

        public virtual Task<Result> SaveChangesAsync(CancellationToken token = default)
            => SaveChangesAsync(null, null, token);

        public virtual Result SaveChanges(Func<Task<Result>>? before,
            Func<Task<Result>>? after = null)
            => AsyncHelper.RunSync(() => SaveChangesAsync(before, after));

        public virtual async Task<Result> SaveChangesAsync(Func<Task<Result>>? before,
            Func<Task<Result>>? after = null, CancellationToken token = default)
        {
            // ReSharper disable once MethodSupportsCancellation
            await _asyncLock.WaitAsync(TimeSpan.FromSeconds(30)).ConfigureAwait(false);

            var logger = LoggerFactory?.CreateLogger<MongoFactory>();
            try
            {
                var t1 = await DbContext.CommitAsync(token);
                var t2 = await DbContext.CommitTransactionAsync(before, after, token);

                return Result.Combine(t1, t2);
            }
            catch (Exception e)
            {
                logger?.LogError("{Message}", e.Message);
                return Result.Fail(e.Message);
            }
            finally
            {
                _asyncLock.Release();
            }
        }

        public virtual void Dispose()
        {
            _asyncLock.Dispose();
        }
    }
}