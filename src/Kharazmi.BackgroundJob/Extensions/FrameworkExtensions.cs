#region

using System;
using System.Collections.Generic;
using System.Linq;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.MemoryStorage;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using Hangfire.Redis;
using Kharazmi.BuilderExtensions;
using Kharazmi.Constants;
using Kharazmi.Extensions;
using Kharazmi.Hangfire.Default;
using Kharazmi.Options.Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

#endregion

namespace Kharazmi.Hangfire.Extensions
{
    /// <summary> </summary>
    public static class FrameworkBuilderExtensions
    {
        /// <summary> </summary>
        public static IConfigurePluginBuilder AddHangfireConfigurePlugin(this IConfigurePluginBuilder builder)
        {
            builder.AddConfigurePluginsFrom(new[] {typeof(AssemblyType).Assembly});
            return builder;
        }

        /// <summary> </summary>
        public static IApplicationBuilder UseHangfire(this IApplicationBuilder app)
        {
            var services = app.ApplicationServices;
            var settings = services.GetSettings();
            var option = settings.Get<HangfireOption>();

            if (option.Enable == false) return app;
            if (option.Enable == false) return app;

            option.Validate();
            if (option.IsValid() == false) return app;

            var serverOptions = option.HangfireServerOption;

            if (serverOptions.IsNull())
                return app;

            serverOptions.Validate();
            if (serverOptions.IsValid() == false) return app;
            
            var jobServerOptions = serverOptions.MapTo<BackgroundJobServerOptions>();
            if (jobServerOptions.IsNull()) return app;
            
            app.UseHangfireServer(jobServerOptions);
            
            if (!option.UseDashboard)
                return app;

            var dashboardOptions = option.MapTo<DashboardOptions>();
            if (dashboardOptions is null)
                return app;

            var routeOptions = services.GetRequiredService<RouteCollection>();

            dashboardOptions.TimeZoneResolver =
                services.GetService<ITimeZoneResolver>() ?? new DefaultTimeZoneResolver();

            if (option.UseAuthorization)
            {
                var authorization = services.GetServices<IDashboardAuthorizationFilter>().ToList();
                if (!authorization.Any())
                {
                    authorization = new List<IDashboardAuthorizationFilter>
                    {
                        new LocalRequestsOnlyAuthorizationFilter(),
                        new JobStoreDashboardAuthorizationFilter()
                    };
                }

                dashboardOptions.Authorization = authorization;
            }

            var storageOptions = services.GetRequiredService<JobStorage>();

            app.Map(new PathString(option.PathMatch),
                builder => builder.UseMiddleware<JobStoreDashboardMiddleware>(
                    storageOptions, dashboardOptions, routeOptions, settings));

            return app;
        }

        internal static void SetConfig(this HangfireOption option, IGlobalConfiguration config,
            IServiceCollection services)
        {
            config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseFilter(new AutomaticRetryAttribute
                {
                    Attempts = option.Attempts,
                    DelaysInSeconds = option.DelaysInSeconds,
                    OnAttemptsExceeded = AttemptsExceededAction.Fail
                })
                .UseFilter(new ProlongExpirationTimeAttribute())
                .UseFilter(new JobExceptionFilterAttribute())
                .UseFilter(new DistributedLockAttribute(TimeSpan.FromSeconds(30)));


            switch (option.JobStorageType)
            {
                case JobStorageTypes.InMemory:
                    var memoryStorageOption = option.HangfireInMemoryStorageOption ;
                    var memoryStorage = memoryStorageOption.MapTo<MemoryStorageOptions>() ?? new MemoryStorageOptions();
                    
                    config.UseMemoryStorage(memoryStorage);
                    break;
                case JobStorageTypes.Redis:

                    var cacheOptionKey = option.HangfireRedisStorageOption;

                    cacheOptionKey.Validate();
                    if (cacheOptionKey.IsValid() == false)
                        goto case JobStorageTypes.InMemory;

                    config.UseRedisStorage(cacheOptionKey.ConnectionString,
                        cacheOptionKey.MapTo<RedisStorageOptions>());

                    break;
                case JobStorageTypes.MongoDb:
                    var mongoOptions = option.HangfireMongoStorageOption;
                    if (mongoOptions.IsNull())
                        goto case JobStorageTypes.InMemory;

                    mongoOptions.Validate();
                    if (mongoOptions.IsValid() == false)
                        goto case JobStorageTypes.InMemory;

                    var mongoStorageOptions = mongoOptions.MapTo<MongoStorageOptions>();

                    if (mongoStorageOptions is null)
                        goto case JobStorageTypes.InMemory;

                    mongoStorageOptions.MigrationOptions = new MongoMigrationOptions
                    {
                        MigrationStrategy = new MigrateMongoMigrationStrategy(),
                        BackupStrategy = new CollectionMongoBackupStrategy()
                    };

                    config.UseMongoStorage(mongoOptions.ConnectionString, mongoStorageOptions);
                    break;
                default:
                    goto case JobStorageTypes.InMemory;
            }

            switch (option.LoggerProvider)
            {
                case "None":
                    break;
                case "Console":
                    config.UseColouredConsoleLogProvider();
                    break;
                case "Default":
                    var loggerFactory = services.BuildServiceProvider().GetService<ILoggerFactory>();
                    config.UseLogProvider(new DefaultLogProvider(loggerFactory));
                    break;
                case "Serilog":
                    config.UseSerilogLogProvider();
                    break;
            }
        }
    }
}