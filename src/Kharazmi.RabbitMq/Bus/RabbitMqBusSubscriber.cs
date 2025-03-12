#region

using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.BuilderExtensions;
using Kharazmi.Bus;
using Kharazmi.Common.Bus;
using Kharazmi.Common.Events;
using Kharazmi.Common.Metadata;
using Kharazmi.Dispatchers;
using Kharazmi.Exceptions;
using Kharazmi.Extensions;
using Kharazmi.Functional;
using Kharazmi.Options.RabbitMq;
using Kharazmi.RabbitMq.Extensions;
using Kharazmi.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using RawRabbit;
using RawRabbit.Common;
using RawRabbit.Configuration.Consume;
using RawRabbit.Configuration.Exchange;
using RawRabbit.Configuration.Queue;
using RawRabbit.Operations.Subscribe.Context;
using RawRabbit.Pipe;
using ExchangeType = RawRabbit.Configuration.Exchange.ExchangeType;
using Retry = RawRabbit.Common.Retry;

#endregion

namespace Kharazmi.RabbitMq.Bus
{
    /// <summary>
    ///
    /// </summary>
    internal class RabbitMqBusSubscriber : IBusSubscriber
    {
        /// <summary></summary>
        private readonly IBusClient _busClient;

        private readonly ILogger<RabbitMqBusSubscriber>? _logger;
        private readonly RabbitMqOption _option;
        private readonly IServiceProvider _sp;
        private readonly int _retries;
        private readonly int _retryInterval;
        private readonly IBusSubscriberStrategy _subscriberStrategy;

        /// <summary></summary>
        public RabbitMqBusSubscriber(IServiceProvider sp)
        {
            _sp = sp;
            _logger = _sp.GetService<ILogger<RabbitMqBusSubscriber>>();
            _busClient = _sp.GetInstance<IBusClientFactory>().BusClient;
            _subscriberStrategy = _sp.GetRequiredService<IBusSubscriberStrategy>();
            _option = _sp.GetSettings().Get<RabbitMqOption>();
            var retryOption = _option.RetryOption;
            _retries = retryOption?.Attempt ?? 0;
            _retryInterval = retryOption?.MaxDelay ?? 0;
        }

        /// <summary></summary>
        /// <param name="onFailed"></param>
        /// <param name="messageConfiguration"></param>
        /// <param name="token"></param>
        /// <typeparam name="TEvent"></typeparam>
        /// <returns></returns>
        public IBusSubscriber SubscribeTo<TEvent>(
            Func<TEvent, Exception, IRejectedDomainEvent>? onFailed = null,
            MessageConfiguration? messageConfiguration = null,
            CancellationToken token = default)
            where TEvent : class, IDomainEvent
        {
            if (messageConfiguration.IsNull() == false)
            {
                AsyncHelper.RunSync(() => SubscribeFromAsync<TEvent>(messageConfiguration,
                    async (@event, domainContext) =>
                    {
                        Result<bool> result;
                        if (_option.WithRequeuing)
                        {
                            result = await TryHandleWithRequeuingAsync(@event, domainContext, async () =>
                                    await _sp.GetInstance<IDomainDispatcher>()
                                        .RaiseAsync(@event, @event.GetType(), domainContext, token), onFailed,
                                _retryInterval,
                                token);

                            if (!result.Failed && result.Value)
                                return Retry.In(TimeSpan.FromSeconds(_retryInterval));
                        }
                        else
                        {
                            result = await TryHandleAsync(@event, domainContext, async () =>
                                        await _sp.GetInstance<IDomainDispatcher>()
                                            .RaiseAsync(@event, @event.GetType(), domainContext, token), onFailed,
                                    _retries,
                                    _retryInterval, token)
                                .ConfigureAwait(false);
                        }

                        if (!result.Failed) return new Ack();
                        if (_option.ThrowExceptionOnSubscribeFailed)
                            throw new RabbitMqSubscriberException(result.Description);
                        return new Reject();
                    }, token));


                return this;
            }

            AsyncHelper.RunSync(() => _busClient.SubscribeAsync<TEvent, DomainMetadata>(
                async (@event, domainContext) =>
                {
                    Result<bool> result;
                    if (_option.WithRequeuing)
                    {
                        result = await TryHandleWithRequeuingAsync(@event, domainContext, async () =>
                            await _sp
                                .GetInstance<IDomainDispatcher>()
                                .RaiseAsync(@event, @event.GetType(), domainContext, token), onFailed, _retries, token);

                        if (!result.Failed && result.Value)
                            return Retry.In(TimeSpan.FromSeconds(_retryInterval));
                    }

                    result = await TryHandleAsync(@event, domainContext, async () => await _sp
                            .GetInstance<IDomainDispatcher>()
                            .RaiseAsync(@event, @event.GetType(), domainContext, token), onFailed, _retries,
                        _retryInterval,
                        token);

                    if (result.Failed) return new Reject();

                    return new Ack();
                }, ConfigSubscribeContext, token));

            return this;
        }

        private Task SubscribeFromAsync<TMessage>(
            MessageConfiguration? messageConfiguration,
            Func<TMessage, DomainMetadata, Task<Acknowledgement>>? subscribeMethod,
            CancellationToken cancellationToken = default)
        {
            var exchangeName = typeof(TMessage).GetExchangeName(_option);
            var exchangeType = typeof(TMessage).GetExchangeType();
            var queueName = typeof(TMessage).GetQueueName(_option);
            var routingKey = typeof(TMessage).GetRoutingKey(_option);
            var queueNameConfig = "";
            var queueNameBindExchange = "";

            var exchangeConfig = messageConfiguration?.ExchangeConfiguration;
            var queueConfig = messageConfiguration?.QueueConfiguration;

            Action<IConsumeConfigurationBuilder> consumeConfigBuilder = consumeConfig => consumeConfig
                .OnExchange(exchangeName)
                .FromQueue(queueName)
                .WithRoutingKey(routingKey);

            Action<IExchangeDeclarationBuilder> exchange = exchangeBuilder => exchangeBuilder
                .WithName(exchangeName)
                .WithType(exchangeType);

            Action<IQueueDeclarationBuilder> queue = queueBuilder => queueBuilder
                .WithName(queueName);


            if (exchangeConfig != null)
            {
                consumeConfigBuilder = builder =>
                {
                    if (exchangeConfig.ExchangeName.IsNotEmpty())
                        exchangeName = exchangeConfig.ExchangeName;

                    if (Enum.GetName(typeof(ExchangeType), exchangeConfig.ExchangeType).IsNotEmpty())
                        exchangeType = exchangeConfig.ExchangeType.MapExchangeType();

                    if (exchangeConfig.RoutingKey.IsNotEmpty())
                        routingKey = exchangeConfig.RoutingKey;

                    if (exchangeConfig.QueueName.IsNotEmpty())
                        queueNameBindExchange = exchangeConfig.QueueName;

                    builder.OnExchange(exchangeName)
                        .FromQueue(queueName)
                        .WithRoutingKey(routingKey)
                        .WithPrefetchCount(exchangeConfig.PrefetchCount)
                        .WithAutoAck(exchangeConfig.AutoAck)
                        .WithNoLocal(exchangeConfig.NoLocal);

                    if (exchangeConfig.ConsumerTag.IsNotEmpty())
                        builder.WithConsumerTag(exchangeConfig.ConsumerTag);

                    if (exchangeConfig.Arguments is null || exchangeConfig.Arguments.Count <= 0) return;
                    foreach (var argument in exchangeConfig.Arguments)
                        builder.WithArgument(argument.Key, argument.Value);
                };

                exchange = exchangeBuilder =>
                {
                    exchangeBuilder
                        .WithName(exchangeName)
                        .WithDurability(exchangeConfig.Durability)
                        .WithAutoDelete(exchangeConfig.AutoDelete);

                    if (Enum.GetName(typeof(ExchangeType), exchangeConfig.ExchangeType).IsNotEmpty())
                        exchangeBuilder.WithType(exchangeConfig.ExchangeType.MapExchangeType());

                    if (exchangeConfig.Arguments is null || exchangeConfig.Arguments.Count <= 0) return;
                    foreach (var argument in exchangeConfig.Arguments)
                        exchangeBuilder.WithArgument(argument.Key, argument.Value);
                };
            }

            if (queueConfig != null)
                queue = queueBuilder =>
                {
                    if (queueConfig.Name.IsNotEmpty())
                        queueNameConfig = queueConfig.Name;

                    queueBuilder.WithName(queueName)
                        .WithDurability(queueConfig.Durability)
                        .WithAutoDelete(queueConfig.AutoDelete);

                    if (queueConfig.NameSuffix.IsNotEmpty())
                        queueBuilder.WithNameSuffix(queueConfig.NameSuffix);

                    if (queueConfig.Arguments is null || queueConfig.Arguments.Count <= 0) return;
                    foreach (var argument in queueConfig.Arguments)
                        queueBuilder.WithArgument(argument.Key, argument.Value);
                };

            if (queueNameConfig.IsNotEmpty() &&
                queueNameBindExchange.IsNotEmpty() &&
                !queueNameConfig.Equals(queueNameBindExchange))
                throw DomainException.For(
                    Result.Fail(
                        $"Invalid Queue Name: {nameof(queueName)}. Queue name is not assailable to the exchange [{exchangeName}]"));

            return _busClient.SubscribeAsync(subscribeMethod, ctx =>
                ctx.UseSubscribeConfiguration(c => c
                    .Consume(consumeConfigBuilder)
                    .FromDeclaredQueue(queue)
                    .OnDeclaredExchange(exchange)), cancellationToken);
        }

        private async Task<Result<bool>> TryHandleAsync<TMessage>(
            TMessage? domainEvent,
            DomainMetadata domainMetadata,
            Func<Task<Result>>? handle,
            Func<TMessage, Exception, IRejectedDomainEvent>? onFailed = null,
            int retries = 0,
            int retryInterval = 0,
            CancellationToken token = default) where TMessage : class, IDomainEvent
        {
            if (domainEvent is null || handle is null) return Result.FailAs<bool>("Invalid  domainEvent");

            var retryCounter = 0;

            var retryPolicy = Policy<Result<bool>>
                .Handle<MessageBusException>()
                .WaitAndRetryAsync(retries, _ => TimeSpan.FromSeconds(retryInterval));

            var attribute = domainEvent.GetType().GetCustomAttribute<EventAttribute>();
            var messageType = attribute != null ? attribute.Name : domainEvent.GetType().Name;

            return await retryPolicy.ExecuteAsync(async () =>
            {
                try
                {
                    token.ThrowIfCancellationRequested();

                    await _subscriberStrategy.BeforeDispatchEventAsync(domainEvent, token);

                    var result = await handle.Invoke();

                    if (result.Failed)
                    {
                        await _subscriberStrategy.OnHandleEventFailedAsync(domainEvent, result, token);
                        return Result.MapToFail(result, false);
                    }

                    await _subscriberStrategy.OnHandleEventSucceedAsync(domainEvent, token);

                    return Result.OkAs(false);
                }
                catch (Exception e)
                {
                    if (e is FrameworkException || e is DomainException)
                    {
                        await _subscriberStrategy.OnDispatchEventFailedAsync(domainEvent, e, domainMetadata,
                            onFailed as Func<IDomainEvent, Exception, IRejectedDomainEvent>, null, token);

                        return Result.FailAs<bool>(
                            $"Unable to handle a domainEvent event with type '{typeof(TMessage).Name}' and domain id '{domainMetadata.DomainId}'");
                    }

                    retryCounter++;

                    if (retryCounter <= retries)
                    {
                        await _subscriberStrategy.OnRetryHandleRejectedEventAsync(domainEvent, retryCounter, token);
                        throw new MessageBusException(e.Message);
                    }

                    await _subscriberStrategy.OnDispatchEventFailedAsync(domainEvent, e, domainMetadata,
                        onFailed as Func<IDomainEvent, Exception, IRejectedDomainEvent>, null, token);


                    return Result.FailAs<bool>(
                        $"Unable to handle a domainEvent with type '{messageType}' with domain id: '{domainMetadata.DomainId}'");
                }
            });
        }


        private async Task<Result<bool>> TryHandleWithRequeuingAsync<TDomainEvent>(
            TDomainEvent? domainEvent,
            DomainMetadata domain,
            Func<Task<Result>>? handle,
            Func<TDomainEvent, Exception, IRejectedDomainEvent>? onFailed = null,
            int retries = 0,
            CancellationToken token = default) where TDomainEvent : class, IDomainEvent
        {
            if (domainEvent is null || handle is null) return Result.FailAs<bool>("Invalid data domainEvent");

            var messageType = domainEvent.GetType().Name;
            var retryCounter = domain.Retries;

            try
            {
                token.ThrowIfCancellationRequested();

                await _subscriberStrategy.BeforeDispatchEventAsync(domainEvent, token);

                var result = await handle.Invoke();

                if (result.Failed)
                {
                    await _subscriberStrategy.OnHandleEventFailedAsync(domainEvent, result, token);
                    return Result.MapToFail<bool>(result);
                }

                await _subscriberStrategy.OnHandleEventSucceedAsync(domainEvent, token);

                return Result.OkAs(false);
            }
            catch (Exception e)
            {
                _logger?.LogError("{Message}", e.Message);

                if (e is FrameworkException || e is DomainException)
                {
                    await _subscriberStrategy.OnDispatchEventFailedAsync(domainEvent, e, domain,
                        onFailed as Func<IDomainEvent, Exception, IRejectedDomainEvent>, null, token);

                    return Result.FailAs<bool>(
                        $"Unable to handle a domainEvent event with type '{typeof(TDomainEvent).Name}' and domain id '{domain.DomainId}'");
                }

                if (retryCounter < retries)
                {
                    await _subscriberStrategy.OnRetryHandleRejectedEventAsync(domainEvent, retryCounter, token);
                    return Result.OkAs(true);
                }

                await _subscriberStrategy.OnDispatchEventFailedAsync(domainEvent, e, domain,
                    onFailed as Func<IDomainEvent, Exception, IRejectedDomainEvent>, null, token);

                return Result.FailAs<bool>(
                    $"Unable to handle a domainEvent with type '{messageType}' with domain id: '{domain.DomainId}'");
            }
        }


        private static void ConfigSubscribeContext(ISubscribeContext context)
        {
            context.UseThrottledConsume(async (func, token) =>
            {
                var semaphore = new SemaphoreSlim(1, 1);
                await semaphore.WaitAsync(token);
                try
                {
                    func?.Invoke();
                }
                finally
                {
                    semaphore.Release();
                }
            });
        }
    }
}