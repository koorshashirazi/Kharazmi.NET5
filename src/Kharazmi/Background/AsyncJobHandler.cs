using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Kharazmi.Background
{
    public interface IAsyncJobHandler
    {
        Task HandlerAsync(IAsyncJob job);
    }

    internal class AsyncJobHandler : IAsyncJobHandler
    {
        private readonly IServiceProvider _provider;
        private readonly ILoggerFactory _loggerFactory;

        public AsyncJobHandler(IServiceProvider provider)
        {
            _provider = provider;
            _loggerFactory = _provider.GetService<ILoggerFactory>() ?? NullLoggerFactory.Instance;
        }

        public Task HandlerAsync(IAsyncJob job)
        {
            var logger = _loggerFactory.CreateLogger<AsyncJobHandler>();
            try
            {
                return job.ExecuteAsync(_provider);
            }
            catch (Exception e)
            {
                logger.LogError("{Message}", e.Message);
                return Task.CompletedTask;
            }
        }
    }
}