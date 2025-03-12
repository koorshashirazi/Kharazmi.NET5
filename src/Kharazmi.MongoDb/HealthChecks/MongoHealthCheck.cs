using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Configuration;
using Kharazmi.Constants;
using Kharazmi.Extensions;
using Kharazmi.Localization.Extensions;
using Kharazmi.Options.Mongo;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Clusters;

namespace Kharazmi.Localization.HealthChecks
{
    internal sealed class MongoHealthCheck : IMongoHealthCheck
    {
        private readonly ISettingProvider _settings;
        private readonly ILogger<MongoHealthCheck>? _logger;
        private static int _attempt;

        internal MongoHealthCheck(ISettingProvider settings, ILogger<MongoHealthCheck>? logger)
        {
            _settings = settings;
            _logger = logger;
        }


        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext? context,
            CancellationToken token = default)
        {
            try
            {
                var options = _settings.Get<MongoOptions>();
                
                var option = options.ChildOptions.ToList()
                    .FirstOrDefault(x => x.HealthCheckOption?.Name == context?.Registration?.Name);

                if (option.IsNull())
                {
                    _logger?.LogError(MessageTemplate.OptionValidation, MessageEventName.OptionsValidation,
                        nameof(MongoHealthCheck), nameof(MongoOption), "MongoOption is null");
                    return HealthCheckResult.Unhealthy("MongoHealthCheck can't handle a null mongoOption");
                }


                try
                {
                    option.Validate();
                    if (!option.IsValid())
                    {
                        var result = option.ValidationResults().AsString();
                        _logger?.LogError(MessageTemplate.OptionValidation, MessageEventName.OptionsValidation,
                            nameof(MongoHealthCheck), nameof(MongoOption), option.ValidationResults().AsString());
                        return HealthCheckResult.Unhealthy(result);
                    }

                    MongoClientSettings clientSettings = option.BuildClientSettings();
                    var database = new MongoClient(clientSettings).GetDatabase(option.Database);

                    _attempt++;

                    if (option.HealthCheckOption?.Attempts > 0 &&
                        _attempt >= option.HealthCheckOption?.Attempts)
                    {
                        _logger?.LogTrace(
                            "The MongoDb is health. Database in use {Database} is active. Attempted to send ping {Attempt} times",
                            option.Database, _attempt);
                        return HealthCheckResult.Healthy(
                            $"The MongoDb is health. Database in use {option.Database} is active. Attempted to send ping {_attempt} times");
                    }

                    if (token.IsCancellationRequested)
                        return HealthCheckResult.Degraded();

                    try
                    {
                        using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));

                        var ping = await database
                            .RunCommandAsync<BsonDocument>(new BsonDocument {{"ping", 1}}, default,
                                tokenSource.Token)
                            .ConfigureAwait(false);

                        if (!ping.Contains("ok") ||
                            (!ping["ok"].IsDouble || Convert.ToDecimal(ping["ok"].AsDouble) != 1m) &&
                            (!ping["ok"].IsInt32 || ping["ok"].AsInt32 != 1))
                        {
                            _logger?.LogError("Ensure MongoDb server {Database} is running: {Ping}", option.Database,
                                ping.ToJson());
                            return HealthCheckResult.Unhealthy(
                                $"Ensure MongoDb server [{option.Database}] is running: {ping.ToJson()}");
                        }

                        if (database.Client.Cluster.Description.State != ClusterState.Connected)
                        {
                            _logger?.LogError("Ensure MongoDb server {Database} is running: {Ping}", option.Database,
                                ping.ToJson());
                            return HealthCheckResult.Unhealthy(
                                $"Ensure MongoDb server [{option.Database}] is running : {ping.ToJson()}");
                        }

                        _logger?.LogTrace("he MongoDb server is health. Database in use {DatabaseName}. Attempted {Attempt} times", option.Database, _attempt);
                        _logger?.LogTrace("The MongoDb is health. Attempted {Attempt} times", _attempt);
                        return HealthCheckResult.Healthy(
                            $"The MongoDb is health. Database in use {option.Database}. Attempted {_attempt} times");
                    }
                    catch (OperationCanceledException ex)
                    {
                        _logger?.LogError("{Message}", ex.Message);
                        if (context != null)
                            return HealthCheckResult.Unhealthy(
                                $"{context.Registration.Name}: Exception {ex.GetType().FullName}", ex);

                        return HealthCheckResult.Unhealthy(
                            $"Ensure MongoDb server [{option.Database}] is running");
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError("{Message}", ex.Message);
                    if (context != null)
                        return HealthCheckResult.Unhealthy(
                            $"{context.Registration.Name}: Exception {ex.GetType().FullName}", ex);

                    return HealthCheckResult.Unhealthy($"Ensure MongoDb server [{option.Database}] is running...");
                }
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy(ex.Message);
            }
        }
    }
}