using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Configuration;
using Kharazmi.Extensions;
using Kharazmi.Options.RabbitMq;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using SslOption = RabbitMQ.Client.SslOption;

namespace Kharazmi.RabbitMq.HealthChecks
{
    internal class RabbitMqHealthCheck : IRabbitMqHealthCheck
    {
        private static int _attempt;
        private IConnection? _connection;
        private readonly ISettingProvider _settingProvider;
        private readonly ILogger<RabbitMqHealthCheck>? _logger;

        public RabbitMqHealthCheck(ISettingProvider settingProvider, ILogger<RabbitMqHealthCheck>? logger)
        {
            _settingProvider = settingProvider;
            _logger = logger;
        }


        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken token = default)
        {
            var options = _settingProvider.Get<RabbitMqOption>();

            try
            {
                if (!options.Enable)
                {
                    return Task.FromResult(HealthCheckResult.Degraded("RabbitMq is disabled"));
                }

                if (!options.IsValid())
                {
                    _logger?.LogError("Invalid rabbitMq options");
                    return Task.FromResult(HealthCheckResult.Unhealthy("Invalid rabbitMq options"));
                }

                _attempt++;

                if (options.HealthCheckOption?.Attempts > 0 &&
                    _attempt >= options.HealthCheckOption?.Attempts)
                {
                    _logger?.LogTrace("The RabbitMq server is active. Attempted to send ping {Attempt} times",
                        _attempt);
                    return Task.FromResult(HealthCheckResult.Healthy(
                        $"The RabbitMq server is active. Attempted to send ping {_attempt} times"));
                }

                if (token.IsCancellationRequested)
                    return Task.FromResult(HealthCheckResult.Degraded());

                EnsureConnection(options);

                if (_connection is null)
                {
                    _logger?.LogWarning("Ensure RabbitMq server is running...");
                    return Task.FromResult(
                        HealthCheckResult.Unhealthy("Ensure RabbitMq server is running..."));
                }


                using (_connection.CreateModel())
                {
                    _logger?.LogTrace("Rabbitmq is active as a message broken");
                    _logger?.LogTrace("The Rabbitmq is health.  For Exchange {Exchange}. Attempted {Attempt} times",
                        _attempt, options.ExchangeName);
                    return Task.FromResult(HealthCheckResult.Healthy(
                        $"The Rabbitmq is health. For Exchange {options.ExchangeName}. Attempted {_attempt} times"));
                }
            }
            catch (Exception e)
            {
                _logger?.LogError("{Message}", e.Message);
                return Task.FromResult(new HealthCheckResult(context.Registration.FailureStatus, exception: e));
            }
        }


        private void EnsureConnection(RabbitMqOption options)
        {
            if (_connection != null) return;

            var factory = new ConnectionFactory
            {
                VirtualHost = options.VirtualHost,
                UserName = options.Username,
                Password = options.Password,
                Port = options.Port,
                HostName = options.HostNames.FirstOrDefault(),
                AutomaticRecoveryEnabled = options.AutomaticRecovery,
                UseBackgroundThreadsForIO = true,
              
            };
            
            if (options.UseSsl)
                factory.Ssl = options.Ssl.MapTo<SslOption>();

            _connection = factory.CreateConnection();
        }
    }
}