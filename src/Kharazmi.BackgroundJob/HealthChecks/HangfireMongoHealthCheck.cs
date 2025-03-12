using System;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Extensions;
using Kharazmi.Options.Hangfire;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Clusters;

namespace Kharazmi.Hangfire.HealthChecks
{
    internal sealed class HangfireMongoHealthCheck : IHangfireMongoHealthCheck
    {
        private readonly HangfireMongoStorageOption? _options;
        private readonly ILogger<HangfireMongoHealthCheck>? _logger;
        private static int _attempt;

        internal HangfireMongoHealthCheck(HangfireMongoStorageOption? mongoOption,
            ILogger<HangfireMongoHealthCheck>? logger)
        {
            _options = mongoOption;
            _logger = logger;
        }


        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext? context,
            CancellationToken token = default)
        {
#warning Fix Messages
            if (_options.IsNull())
            {
                // TODO use MessageTemplate
                _logger?.LogError("Invalid MongoDb Options");
                return HealthCheckResult.Unhealthy("Invalid MongoDb Options");
            }

            try
            {
                if (!_options.IsValid())
                {
                    _logger?.LogError("Invalid MongoDb Options");
                    return HealthCheckResult.Unhealthy("Invalid MongoDb Options");
                }

                var databaseName = new MongoUrlBuilder(_options.ConnectionString).DatabaseName;
                var mongoClientSettings = MongoClientSettings.FromConnectionString(_options.ConnectionString);
                var client = new MongoClient(mongoClientSettings);
                var database = client.GetDatabase(databaseName);

                if (database.IsNull())
                    throw new MongoClientException($"Can't get database with name {databaseName}");

                _attempt++;

                if (_options.HealthCheckOption?.Attempts > 0 &&
                    _attempt >= _options.HealthCheckOption?.Attempts)
                {
                    _logger?.LogTrace(
                        "he MongoDb server is health. Database in use {DatabaseName}. Attempted {Attempt} times",
                        databaseName, _attempt);
                    return HealthCheckResult.Healthy(
                        $"he MongoDb server is health. Database in use {databaseName}. Attempted {_attempt} times");
                }

                if (token.IsCancellationRequested)
                    return HealthCheckResult.Degraded();

                try
                {
                    using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(3));

                    var ping = await database
                        .RunCommandAsync<BsonDocument>(new BsonDocument {{"ping", 1}}, default,
                            tokenSource.Token)
                        .ConfigureAwait(false);

                    if (!ping.Contains("ok") ||
                        (!ping["ok"].IsDouble || Convert.ToDecimal(ping["ok"].AsDouble) != 1m) &&
                        (!ping["ok"].IsInt32 || ping["ok"].AsInt32 != 1))
                    {
                        _logger?.LogError("Ensure MongoDb server {Database} is running: {Ping}", databaseName,
                            ping.ToJson());
                        return HealthCheckResult.Unhealthy(
                            $"Ensure MongoDb server [{databaseName}] is running: {ping.ToJson()}");
                    }

                    if (database.Client.Cluster.Description.State != ClusterState.Connected)
                    {
                        _logger?.LogError("Ensure MongoDb server {Database} is running: {Ping}", databaseName,
                            ping.ToJson());
                        return HealthCheckResult.Unhealthy(
                            $"Ensure MongoDb server [{databaseName}] is running : {ping.ToJson()}");
                    }

                    _logger?.LogTrace("The MongoDb is health. Database in use {Database}. Attempted {Attempt} times", databaseName, _attempt);
                    return HealthCheckResult.Healthy($"The MongoDb server is health. Database in use {databaseName}. Attempted {_attempt} times");
                }
                catch (OperationCanceledException ex)
                {
                    _logger?.LogError("{Message}", ex.Message);
                    if (context != null)
                        return HealthCheckResult.Unhealthy(
                            $"{context.Registration.Name}: Exception {ex.GetType().FullName}", ex);

                    return HealthCheckResult.Unhealthy(
                        $"Ensure MongoDb server [{databaseName}] is running");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError("{Message}", ex.Message);
                if (context != null)
                    return HealthCheckResult.Unhealthy(
                        $"{context.Registration.Name}: Exception {ex.GetType().FullName}", ex);

                return HealthCheckResult.Unhealthy($"Ensure MongoDb server is running...");
            }
        }
    }
}