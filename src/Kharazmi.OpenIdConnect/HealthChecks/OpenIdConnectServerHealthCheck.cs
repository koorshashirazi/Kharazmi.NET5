using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Configuration;
using Kharazmi.Constants;
using Kharazmi.Extensions;
using Kharazmi.Http;
using Kharazmi.Options.Cookie;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Kharazmi.OpenIdConnect.HealthChecks
{
    internal class OpenIdConnectServerHealthCheck : IOpenIdConnectServerHealthCheck
    {
        private readonly ISettingProvider _settingProvider;
        private readonly IHttpClientFactory? _factory;
        private readonly ILogger<OpenIdConnectServerHealthCheck>? _logger;
        private static int _attempt;

        /// <summary>_</summary>
        /// <param name="settingProvider"></param>
        /// <param name="factory"></param>
        /// <param name="logger"></param>
        public OpenIdConnectServerHealthCheck(
            ISettingProvider settingProvider,
            [AllowNull] IHttpClientFactory? factory,
            [AllowNull] ILogger<OpenIdConnectServerHealthCheck> logger)
        {
            _settingProvider = settingProvider;
            _factory = factory;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext? context,
            CancellationToken token = default)
        {
            try
            {
                var options = _settingProvider.Get<ExtendedOpenIdOption>();
                if (options.Enable == false)
                {
                    return HealthCheckResult.Degraded("OpenId connect is disable");
                }

                options.Validate();
                if (!options.IsValid())
                {
                    _logger?.LogError(MessageTemplate.CanNotUseOpenIdConnect, MessageEventName.OpenIdConnect,
                        nameof(OpenIdConnectServerHealthCheck));
                    return HealthCheckResult.Unhealthy(
                        MessageHelper.CanNotUseOpenId(nameof(OpenIdConnectServerHealthCheck)));
                }

                _attempt++;

                if (options.HealthCheckOption?.Attempts > 0 && _attempt >= options.HealthCheckOption?.Attempts)
                {
                    _logger.LogWarning("The authority server is active. Attempted to send ping {Attempt} times",
                        _attempt);
                    return HealthCheckResult.Degraded(
                        $"The authority server is active. Attempted to send ping {_attempt} times");
                }

                if (token.IsCancellationRequested)
                {
                    _logger.LogWarning("Cancellation requested");
                    return HealthCheckResult.Degraded("Cancellation requested");
                }

                if (string.IsNullOrWhiteSpace(options.AuthorityUrl))
                {
                    _logger?.LogError("Invalid AuthorityUrl");
                    return HealthCheckResult.Unhealthy("Invalid AuthorityUrl");
                }

                var httpClient = _factory?.GetOrCreate(options.AuthorityUrl);

                if (httpClient is null)
                {
                    _logger?.LogError("Can't not invoke any implementation of {IHttpClientFactory}",
                        typeof(IHttpClientFactory).FullName);
                    return HealthCheckResult.Unhealthy(
                        $"Can't not invoke any implementation of {typeof(IHttpClientFactory).FullName}");
                }


                var response = await httpClient.GetAsync(options.DiscoverEndpoint, token)
                    .WithCancellationTokenAsync(token);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync(token);

                    _logger?.LogError(
                        "Discover endpoint is not responding with 200 OK, the current status is {StatusCode} and the content {ErrorContent}",
                        response.StatusCode, errorContent);
                    return HealthCheckResult.Unhealthy(
                        $"Discover endpoint is not responding with 200 OK, the current status is {response.StatusCode} and the content {errorContent}");
                }

                _logger?.LogTrace("The authority server is accessible");
                _logger?.LogTrace("The authority server is health. Attempted {Attempt} times", _attempt);
                return HealthCheckResult.Healthy(
                    $"The authority server is health. Authority Url {options.AuthorityUrl}. Attempted {_attempt} times");
            }
            catch (Exception ex)
            {
                _logger?.LogError("{Message}", ex.Message);
                return HealthCheckResult.Unhealthy(ex.Message);
            }
        }
    }
}