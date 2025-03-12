using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Configuration;
using Kharazmi.Extensions;
using Kharazmi.Mvc.MailServer;
using Kharazmi.Options;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Kharazmi.Mvc.HealthChecks
{
    internal class MailServerHealthCheck : IMailServerHealthCheck
    {
        private static int _attempt;
        private const string Subject = "Mail server health checking...";
        private const string Name = "Health Check User";
        private const string Body = "Test mail server";

        private readonly ISettingProvider _settingProvider;
        private readonly ILogger<MailServerHealthCheck>? _logger;

        public MailServerHealthCheck(
            ISettingProvider settingProvider,
            [AllowNull] ILogger<MailServerHealthCheck>? logger)
        {
            _settingProvider = settingProvider;
            _logger = logger;
        }


        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext? context, CancellationToken token = default)
        {
            try
            {
                var options = _settingProvider.Get<EmailServerOption>();
                if (options.Enable == false)
                {
                    return HealthCheckResult.Degraded("EmailServer is disabled");
                }

                options.Validate();
                if (!options.IsValid())
                {
                    _logger?.LogError("Invalid email server options is configured");
                    return HealthCheckResult.Unhealthy("Invalid Email server options");
                }

                _attempt++;

                if (options.HealthCheckOption?.Attempts > 0 &&
                    _attempt >= options.HealthCheckOption?.Attempts)
                {
                    _logger?.LogTrace("Max Attempts");
                    return HealthCheckResult.Healthy("Max Attempts");
                }
                
                if (token.IsCancellationRequested)
                {
                    _logger?.LogWarning("Cancellation Requested");
                    return HealthCheckResult.Degraded("Cancellation Requested");
                }
            
                if (options.SmtpUsername.IsEmpty())
                {
                    _logger?.LogError("Invalid Smtp Username");
                    return HealthCheckResult.Unhealthy("Invalid Smtp Username");
                }


                var message = EmailMessageBuilder.Create()
                    .WithSender(new MailAddress
                    {
                        Name = Name,
                        Email = options.SmtpUsername
                    })
                    .WithReceiver(new MailAddress
                    {
                        Name = Name,
                        Email = options.SmtpUsername
                    })
                    .WithSubject(Subject)
                    .WithStringBody(Body)
                    .Build();

                if (options.UsePickupFolder)
                {
                    if (options.PickupFolder.IsEmpty())
                    {
                        _logger?.LogError("Invalid {PickupFolder} path", options.PickupFolder);
                        return HealthCheckResult.Unhealthy("Invalid {PickupFolder} path");
                    }

                    const int maxBufferSize = 0x10000; // 64K.

                    await using var stream = new FileStream(
                        Path.Combine(options.PickupFolder, $"email-{Guid.NewGuid():N}.eml"),
                        FileMode.CreateNew, FileAccess.Write, FileShare.None,
                        maxBufferSize, true);

                    await message.WriteToAsync(stream, token).WithCancellationTokenAsync(token);
                }
                else
                {
                    using var client = new SmtpClient();

                    if (options.LocalDomain.IsNotEmpty())
                        client.LocalDomain = options.LocalDomain;

                    await client.ConnectAsync(options.SmtpServer, options.SmtpPort, cancellationToken: token);

                    if (options.SmtpUsername.IsNotEmpty() && options.SmtpPassword.IsNotEmpty())
                        await client.AuthenticateAsync(options.SmtpUsername, options.SmtpPassword, token)
                            .WithCancellationTokenAsync(token);

                    // await client.SendAsync(message, token);
                }

                _logger?.LogTrace("The email server is active. Attempted to send email {Attempt} times", _attempt);
                return HealthCheckResult.Healthy($"The email server is active. Attempted to send email {_attempt} times");
            }
            catch (Exception e)
            {
                _logger?.LogError("{Message}", e.Message);
                return context != null
                    ? new HealthCheckResult(context.Registration.FailureStatus, exception: e)
                    : HealthCheckResult.Unhealthy(e.Message);
            }
        }
    }
}