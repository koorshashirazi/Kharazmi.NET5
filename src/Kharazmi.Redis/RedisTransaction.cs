using System;
using System.Threading.Tasks;
using Kharazmi.Configuration;
using Kharazmi.Dependency;
using Kharazmi.Redis.Extensions;
using StackExchange.Redis;

namespace Kharazmi.Redis
{
    /// <summary>_</summary>
    public interface IRedisTransaction
    {
        /// <summary>_</summary>
        /// <param name="optionKey"></param>
        /// <param name="execute"></param>
        /// <returns></returns>
        Task TransactionAsync(string optionKey, Func<ITransaction, Task[]> execute);
    }


    internal class NullRedisTransaction : IRedisTransaction, INullInstance
    {
        public Task TransactionAsync(string optionKey, Func<ITransaction, Task[]> execute)
            => Task.CompletedTask;
    }

    internal class RedisTransaction : IRedisTransaction
    {
        private readonly ISettingProvider _settingProvider;
        private readonly IRedisPool _pool;

        public RedisTransaction(ServiceFactory<IRedisPool> factory, ISettingProvider settingProvider)
        {
            _settingProvider = settingProvider;
            _pool = factory.Instance();
        }

        public async Task TransactionAsync(string optionKey, Func<ITransaction, Task[]> execute)
        {
            var option = _settingProvider.GetRedisOption(optionKey);
            var redisDatabase = _pool.RedisDatabase(option);
            var database = redisDatabase?.Database;
            var shouldCommit = false;

            ITransaction transaction;
            if (database is ITransaction tran)
            {
                transaction = tran;
            }
            else if (database is { } db)
            {
                transaction = db.CreateTransaction();
                shouldCommit = true;
            }
            else
            {
                throw new RedisException("The database instance type is not supported");
            }

            var tasks = execute.Invoke(transaction);

            if (shouldCommit && !await transaction.ExecuteAsync().ConfigureAwait(false))
                throw new Exception("The transaction has failed");

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
    }
}