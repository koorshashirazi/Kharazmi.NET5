#region

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Common.Metadata;
using Kharazmi.Extensions;
using Kharazmi.Functional;
using Kharazmi.Options;
using Kharazmi.Options.Domain;
using Kharazmi.Retries;
using Microsoft.Extensions.Logging;

#endregion

namespace Kharazmi.Pipelines
{
    internal class DomainRetryHandler : IRetryHandler
    {
        private readonly ILogger<DomainRetryHandler>? _logger;
        private readonly IDomainRetryLogProvider _retryLogProvider;
        private IRetryOptions _options = new RetryOption();

        public DomainRetryHandler(
            ILogger<DomainRetryHandler>? logger,
            IDomainRetryLogProvider retryLogProvider)
        {
            _logger = logger;
            _retryLogProvider = retryLogProvider;
        }


        public async Task<Result<TResult>> HandlerAsync<TResult>(IRetryOptions? retryOption,
            Func<Task<TResult>> invokeMethod,
            DomainMetadata context,
            CancellationToken token = default)
        {
            _options = retryOption ?? new RetryOption();
            var retryCounter = 0;
            while (true)
            {
                var retryLogEvent = new RetryEventLog(context, nameof(DomainRetryHandler));
                try
                {
                    var (before, beforeArgs) = _retryLogProvider.BeforeHandleMessage(retryLogEvent);
                    if (before.IsNotEmpty())
                        _logger.LogTrace(before, beforeArgs);

                    token.ThrowIfCancellationRequested();

                    var stopwatch = new Stopwatch();
                    stopwatch.Start();

                    var result = await invokeMethod.Invoke();

                    stopwatch.Stop();

                    retryLogEvent.TotalSeconds = stopwatch.Elapsed.TotalSeconds;

                    var (after, afterArgs) = _retryLogProvider.AfterHandleMessage(retryLogEvent);
                    if (after.IsNotEmpty())
                        _logger.LogInformation(after, afterArgs);

                    return Result.OkAs(result);
                }
                catch (Exception e)
                {
                    var (exception, exArgs) = _retryLogProvider.OnException(retryLogEvent, e);
                    if (exception.IsNotEmpty())
                        _logger.LogError(exception, exArgs);

                    retryCounter++;

                    var counter = retryCounter;
                    var retry = Retry.When(
                        () => _options.RetryOnExceptionTypes.Any(x => e.GetType().Name == x) && CanRetry(counter),
                        RetryDelay.PickDelay());

                    if (!retry.ShouldBeRetried) return Result.FailAs<TResult>(e.Message);

                    context.IncreaseRetrying();

                    retryLogEvent.Attempt = retryCounter;

                    var (retryTemplate, retryArgs) = _retryLogProvider.OnRetry(retryLogEvent, e);
                    if (retryTemplate.IsNotEmpty())
                        _logger.LogTrace(retryTemplate, retryArgs);

                    await Task.Delay(retry.RetryAfter, token).ConfigureAwait(false);
                }
            }
        }

        public async Task<Result> HandlerAsync(IRetryOptions? retryOption, Func<Task> invokeMethod,
            DomainMetadata context,
            CancellationToken token = default)
        {
            _options = retryOption ?? new RetryOption();
            var retryCounter = 0;
            while (true)
            {
                var retryLogEvent = new RetryEventLog(context, nameof(DomainRetryHandler));

                try
                {
                    token.ThrowIfCancellationRequested();

                    var stopwatch = new Stopwatch();
                    stopwatch.Start();

                    var (before, beforeArgs) = _retryLogProvider.BeforeHandleMessage(retryLogEvent);
                    if (before.IsNotEmpty())
                        _logger.LogTrace(before, beforeArgs);

                    await invokeMethod.Invoke();

                    stopwatch.Stop();

                    retryLogEvent.TotalSeconds = stopwatch.Elapsed.TotalSeconds;

                    var (after, afterArgs) = _retryLogProvider.AfterHandleMessage(retryLogEvent);
                    if (after.IsNotEmpty())
                        _logger.LogInformation(after, afterArgs);

                    return Result.Ok();
                }
                catch (Exception e)
                {
                    var (exception, exArgs) = _retryLogProvider.OnException(retryLogEvent, e);
                    if (exception.IsNotEmpty())
                        _logger.LogError(exception, exArgs);

                    retryCounter++;

                    var counter = retryCounter;
                    var retry = Retry.When(
                        () => _options.RetryOnExceptionTypes.Any(x => e.GetType().Name == x) && CanRetry(counter),
                        RetryDelay.PickDelay());

                    if (!retry.ShouldBeRetried) return Result.Fail(e.Message);

                    context.IncreaseRetrying();

                    retryLogEvent.Attempt = retryCounter;

                    var (retryTemplate, retryArgs) = _retryLogProvider.OnRetry(retryLogEvent, e);
                    if (retryTemplate.IsNotEmpty())
                        _logger.LogTrace(retryTemplate, retryArgs);

                    await Task.Delay(retry.RetryAfter, token).ConfigureAwait(false);
                }
            }
        }

        private RetryDelay RetryDelay =>
            RetryDelay.Between(TimeSpan.FromSeconds(_options.MinDelay), TimeSpan.FromSeconds(_options.MaxDelay));

        private bool CanRetry(int retryCount)
            => RetryDelay.PickDelay() > TimeSpan.Zero && retryCount >= 0 && retryCount <= _options.Attempt;
    }
}