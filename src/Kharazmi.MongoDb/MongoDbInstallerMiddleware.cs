using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Configuration;
using Kharazmi.Constants;
using Kharazmi.Extensions;
using Kharazmi.Options.Mongo;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Kharazmi.Localization
{
    internal class MongoDbInstallerMiddleware : IMiddleware
    {
        private readonly IEnumerable<IMongoDbInstaller> _installers;
        private readonly ISettingProvider _settingProvider;
        private readonly ILogger<MongoDbInstallerMiddleware>? _logger;
        private readonly SemaphoreSlim _asyncLock = new(1, 1);
        private Task? _initializationInstaller;

        public MongoDbInstallerMiddleware(
            IHostApplicationLifetime lifetime,
            IEnumerable<IMongoDbInstaller> installers,
            ISettingProvider settingProvider,
            ILogger<MongoDbInstallerMiddleware>? logger)
        {
            _installers = installers;
            _settingProvider = settingProvider;
            _logger = logger;

            var tokenRegistration = new CancellationTokenRegistration();
            lifetime.ApplicationStarted.Register(() =>
            {
                _initializationInstaller = InitializeAsync(lifetime.ApplicationStopping);
                tokenRegistration.Dispose();
            });
        }

        private async Task InitializeAsync(CancellationToken cancellationToken)
        {
            // ReSharper disable once MethodSupportsCancellation
            await _asyncLock.WaitAsync();
            try
            {
                var options = _settingProvider.Get<MongoOptions>();
                if (options.Enable == false || options.UseDatabaseInstaller == false)
                    return;

                var mongoOptions = options.ChildOptions
                    .Where(op => op.DocumentTypesAssembly != DatabaseMigrationStrategy.AutomaticDetectionAssembly)
                    .ToList();

                foreach (var installer in _installers)
                {
                    if (installer.IsNull())
                    {
                        _logger?.LogError(MessageTemplate.NotRegisterService, MessageEventName.MongoDbInstaller,
                            nameof(MongoDbInstallerMiddleware), typeof(IMongoDbInstaller));
                        return;
                    }

                    var mongoOption = mongoOptions.FirstOrDefault(x =>
                    {
                        if (x.AssemblyDbInstaller.IsEmpty()) return default;
                        return Assembly.Load(x.AssemblyDbInstaller) ==
                               Assembly.Load(installer.AssemblyDbInstaller);
                    });

                    if (mongoOption is null)
                    {
                        _logger?.LogError(MessageTemplate.NotFoundServiceTypeInAssembly,
                            MessageEventName.MongoDbInstaller,
                            nameof(MongoDbInstallerMiddleware), typeof(IMongoDbInstaller),
                            installer.AssemblyDbInstaller);
                        continue;
                    }

                    installer.SetDatabaseTo(mongoOption);
                    await installer.ExecuteAsync(cancellationToken);
                }
            }
            catch (Exception e)
            {
                _logger?.LogError("{Message}", e.Message);
            }
            finally
            {
                _asyncLock.Release();
            }
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var task = _initializationInstaller;
            if (task != null)
            {
                await task;
                _initializationInstaller = null;
            }

            await next(context);
        }
    }
}