using Kharazmi.Background;
using Kharazmi.Configuration;
using Kharazmi.Dependency;
using Kharazmi.Extensions;
using Kharazmi.HealthChecks;
using Kharazmi.Http;
using Kharazmi.Options;
using Kharazmi.Pipelines;
using Kharazmi.Retries;
using Kharazmi.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace Kharazmi.ConfigurePlugins
{
    internal class CoreConfigurePlugin : IConfigurePlugin
    {
        public string PluginName => nameof(CoreConfigurePlugin);

        public void Configure(ISettingProvider settings)
        {
            var option = settings.Get<ApplicationOption>();
            settings.UpdateOption(option);
        }

        public void Initialize(IServiceCollection services, ISettingProvider settings)
        {
            services.AddSingleton<IGlobalCancellationToken, GlobalScopedCancellationToken>();
            services.AddScoped<IScopedCancellationToken, ScopedCancellationToken>();
            services.AddSingleton<IHealthChecker, HealthChecker>();
            services.ReplaceService<IRetryHandler, DomainRetryHandler>(ServiceLifetime.Transient);
            services.ReplaceService<IDynamicResolver, DynamicResolver>(ServiceLifetime.Transient);
            AddDefaultBackgroundJob(services);
            services.ReplaceService<IHttpClientFactory, HttpClientFactory>(ServiceLifetime.Singleton);
        }

        private static void AddDefaultBackgroundJob(IServiceCollection services)
        {
            services.ReplaceService<IAsyncJobHandler, AsyncJobHandler>(ServiceLifetime.Transient);
            services.ReplaceService<IBackgroundJob, BackgroundJob>(ServiceLifetime.Singleton);
            services.RegisterWithFactory<IBackgroundService, BackgroundService>(ServiceLifetime.Singleton);
            services.AddHostedService<BackgroundJobWorker>();
            services.AddHostedService<BackgroundScheduledJobWorker>();
        }
    }
}