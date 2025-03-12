using System;
using System.Collections.Generic;
using System.Threading;
using Kharazmi.BuilderExtensions;
using Kharazmi.Bus;
using Kharazmi.Common.Bus;
using Kharazmi.Common.Events;
using Kharazmi.Common.Metadata;
using Kharazmi.Dispatchers;
using Kharazmi.Exceptions;
using Kharazmi.Extensions;
using Kharazmi.Functional;
using Kharazmi.Json;
using Kharazmi.Options.Redis;
using Kharazmi.Redis.Extensions;
using Kharazmi.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using StackExchange.Redis;

namespace Kharazmi.Redis.Channels
{
    internal class RedisBusSubscriber : IBusSubscriber
    {
        private RedisDbOption _option;
        private readonly ILogger<RedisBusSubscriber>? _logger;
        private readonly IServiceProvider _sp;
        private readonly IBusSubscriberStrategy _subscriberStrategy;

        public RedisBusSubscriber(IServiceProvider sp)
        {
            _sp = sp;
            _subscriberStrategy = _sp.GetRequiredService<IBusSubscriberStrategy>();
            _logger = _sp.GetService<ILogger<RedisBusSubscriber>>();
            _option = _sp.GetSettings().GetRedisOption("");
        }

        private ISubscriber PubSub => _sp.GetInstance<IPubSubFactory>().GetPubSub(_option);

        public IBusSubscriber SubscriberFor(string? optionKey)
        {
            _option = _option = _sp.GetSettings().GetRedisOption(optionKey);
            return this;
        }

        public IBusSubscriber SubscribeTo<TDomainEvent>(
            Func<TDomainEvent, Exception, IRejectedDomainEvent>? onFailed = null,
            MessageConfiguration? messageConfiguration = null,
            CancellationToken token = default)
            where TDomainEvent : class, IDomainEvent
        {
#warning TODO Use Conventions
            var channelName = typeof(TDomainEvent).ChannelName(_option.ChannelPrefix);
            AsyncHelper.RunSync(() => PubSub.SubscribeAsync(channelName, async (_, val) =>
            {
                if (!val.HasValue)
                {
                    _logger?.LogError(
                        "Channel event: '{Name}' not exist", typeof(TDomainEvent).Name);
                    return;
                }

                var channelEvent = Serializer.Deserialize<ChannelEvent>(val);
                var domainEvent = channelEvent?.DomainEvent.Deserialize<TDomainEvent>();
                var metadata = channelEvent?.DomainMetadata.Deserialize<Dictionary<string, string>>();
                var domainContext = DomainMetadata.Empty;
                domainContext.AddRange(metadata);

                if (domainEvent is null)
                {
                    _logger?.LogTrace(
                        "Channel event: '{Name}' not exist", typeof(TDomainEvent).Name);
                    return;
                }

                var retryCounter = 0;
                var retryOption = _option.RetryOption;
                var retryPolicy = Policy<Result>
                    .Handle<RedisChannelException>()
                    .WaitAndRetryAsync(retryOption.Attempt,
                        _ => TimeSpan.FromSeconds(retryOption.MaxDelay));

                var eventId = domainEvent.MessageId;

                var resultException = await retryPolicy.ExecuteAsync(async () =>
                {
                    try
                    {
                        token.ThrowIfCancellationRequested();

                        await _subscriberStrategy.BeforeDispatchEventAsync(domainEvent, token);

                        var result = await _sp.GetInstance<IEventDispatcher>()
                            .RaiseAsync(domainEvent,domainEvent.GetType(), domainContext, token).ConfigureAwait(false);

                        if (result.Failed)
                        {
                            await _subscriberStrategy.OnHandleEventFailedAsync(domainEvent, result, token);
                            return result;
                        }

                        await _subscriberStrategy.OnHandleEventSucceedAsync(domainEvent, token);

                        return Result.Ok();
                    }
                    catch (Exception e)
                    {
                        if (e is FrameworkException || e is DomainException)
                        {
                            await _subscriberStrategy.OnDispatchEventFailedAsync(domainEvent, e, domainContext,
                                onFailed as Func<IDomainEvent, Exception, IRejectedDomainEvent>, _option, token);

                            return Result.Fail(
                                $"Unable to handle a channel event: '{typeof(TDomainEvent).Name}' with event id: '{eventId}'");
                        }

                        retryCounter++;

                        await _subscriberStrategy.OnRetryHandleRejectedEventAsync(domainEvent, retryCounter, token);

                        if (retryCounter <= retryOption.Attempt) throw new RedisChannelException(e.Message);

                        await _subscriberStrategy.OnDispatchEventFailedAsync(domainEvent, e, domainContext,
                            onFailed as Func<IDomainEvent, Exception, IRejectedDomainEvent>, _option, token);

                        return Result.Fail(
                            $"Unable to handle a channel event type '{typeof(TDomainEvent).Name}' with event id: '{eventId}'");
                    }
                });

                if (!resultException.Failed) return;

                _logger?.LogError("Can't handling a channel event with name '{ChannelType}'",
                    typeof(TDomainEvent).Name);

                if (_option.ThrowExceptionOnSubscribeFailed)
                    throw new RedisSubscriberException(
                        "Can't handling a channel event with name '{typeof(TChannelEvent).Name}'");
            }));

            return this;
        }
    }
}