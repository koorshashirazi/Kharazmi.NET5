using System;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Helpers;
using Kharazmi.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Kharazmi.Background
{
    public abstract class Scheduler : Microsoft.Extensions.Hosting.BackgroundService
    {
        private Timer? _timer;
        private readonly CancellationTokenSource _stoppingToken = new();

        protected DateTime Started { get; private set; }
        protected ILogger Logger { get; }
        protected IServiceProvider Services { get; }
        protected TimeSpan Delay { get; private set; }
        protected TimeSpan FirstRun { get; private set; }

        protected Scheduler(IServiceProvider services)
        {
            Services = services;
            var loggerFactory = services.GetService<ILoggerFactory>() ?? NullLoggerFactory.Instance;
            Logger = loggerFactory.CreateLogger<Scheduler>();
            Delay = TimeSpan.Zero;
            FirstRun = TimeSpan.Zero;
            Started = DateTimeHelper.DateTimeUtcNow;
        }


        public override Task StartAsync(CancellationToken cancellationToken)
        {
            Started = DateTimeHelper.DateTimeUtcNow;
            return base.StartAsync(cancellationToken);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _timer = new Timer(ExecuteJob, stoppingToken, FirstRun, TimeSpan.FromMilliseconds(-1));
            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken stoppingToken)
        {
            ReleaseResources();
            return base.StopAsync(stoppingToken);
        }

        protected abstract Task TryExecuteJobAsync(IServiceProvider sp, DateTime now,
            CancellationToken stoppingToken);

        protected Scheduler SetFirstRun(TimeSpan value)
        {
            FirstRun = value;
            return this;
        }

        protected Scheduler SetDelay(TimeSpan value)
        {
            Delay = value;
            return this;
        }


        public Scheduler ResetTo(DateTime now)
        {
            Started = now;
            return this;
        }

        #region Helpers

        private void ExecuteJob(object? state)
        {
            _timer?.Change(Timeout.Infinite, 0);
            AsyncHelper.RunSync(() => ExecuteJobAsync(state));
        }

        private async Task ExecuteJobAsync(object? state)
        {
            try
            {
                if (state is not CancellationToken stoppingToken)
                    stoppingToken = _stoppingToken.Token;

                using var scope = Services.CreateScope();
                await TryExecuteJobAsync(scope.ServiceProvider, DateTimeHelper.DateTimeUtcNow, stoppingToken);
            }
            catch (Exception ex)
            {
                Logger.LogError("{Message}", ex.Message);
            }
            finally
            {
                _timer?.Change(Delay, TimeSpan.FromMilliseconds(-1));
            }
        }

        #endregion

        private void ReleaseResources()
        {
            _timer?.Change(Timeout.Infinite, 0);
            _stoppingToken.Cancel();
            _timer?.Dispose();
        }
    }
}