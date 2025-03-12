#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.BuilderExtensions;
using Kharazmi.Bus;
using Kharazmi.Common.Domain;
using Kharazmi.Common.Events;
using Kharazmi.Common.Metadata;
using Kharazmi.Configuration;
using Kharazmi.Constants;
using Kharazmi.Dispatchers;
using Kharazmi.Domain;
using Kharazmi.Exceptions;
using Kharazmi.Extensions;
using Kharazmi.Functional;
using Kharazmi.Guard;
using Kharazmi.Json;
using Kharazmi.Messages;
using Kharazmi.Models;
using Kharazmi.RealTime;
using Kharazmi.Retries;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;

#endregion

namespace Kharazmi.Handlers
{
    public abstract class MessageHandler<TMessage>
        where TMessage : class, IMessage
    {
        protected readonly ILoggerFactory LoggerFactory;
        protected Stopwatch Stopwatch { get; set; } = Stopwatch.StartNew();
        protected string MessageName { get; set; } = typeof(TMessage).Name;
        protected int RetryCounter { get; private set; }
        protected IRetryHandler RetryHandler { get; }
        protected TMessage Message { get; set; }
        protected IDomainDispatcher DomainDispatcher { get; }
        protected IDomainNotificationHandler DomainNotifier { get; }
        protected IEventProcessor EventProcessor { get; }
        protected IHubClientPublisher HubClientPublisher { get; }

        protected DomainMetadata DomainMetadata { get; set; } =
            DomainMetadata.Empty;

        protected IDomainMetadataAccessor DomainMetadataAccessor { get; }
        public ISettingProvider Settings { get; }

#pragma warning disable 8618
        protected MessageHandler(IServiceProvider sp)
#pragma warning restore 8618
        {
            RetryCounter = 0;
            Settings = sp.GetSettings();
            LoggerFactory = sp.GetService<ILoggerFactory>() ?? NullLoggerFactory.Instance;
            DomainDispatcher = sp.GetInstance<IDomainDispatcher>();
            DomainNotifier = sp.GetSafeRequiredService<IDomainNotificationHandler>();
            DomainMetadataAccessor = sp.GetInstance<IDomainMetadataAccessor>();
            EventProcessor = sp.GetInstance<IEventProcessor>();
            HubClientPublisher = sp.GetInstance<IHubClientPublisher>();
            RetryHandler = sp.GetRequiredService<IRetryHandler>();
        }


        protected virtual Task<Result> OnMessageProcessingAsync(
            IAggregateRoot aggregateRoot,
            Func<IEnumerable<IUncommittedEvent>, IEnumerable<IDomainEvent>> integrationEventMapper,
            Func<IEnumerable<IUncommittedEvent>, IEnumerable<IDomainEvent>>? internalEventMapper = null,
            CancellationToken token = default)
            => EventProcessor.ProcessAsync(aggregateRoot.UncommittedEvents, integrationEventMapper, internalEventMapper,
                DomainMetadata, token);


        protected virtual async Task<Result> ProcessMessageAsync(
            IAggregateRoot aggregateRoot,
            Func<IEnumerable<IUncommittedEvent>, IEnumerable<IDomainEvent>> integrationEventMapper,
            Func<IEnumerable<IUncommittedEvent>, IEnumerable<IDomainEvent>>? internalEventMapper = null,
            CancellationToken token = default)
        {
            aggregateRoot.NotNull(nameof(aggregateRoot));

            var result =
                await OnMessageProcessingAsync(aggregateRoot, integrationEventMapper, internalEventMapper, token);
            AddDomainResult(result);
            return await GetResultAsync();
        }

        protected virtual async Task<Result> ProcessMessageAsync(
            IAggregateRoot aggregateRoot,
            Func<IAggregateRoot, Task<Result>> beforeMessageProcess,
            Func<IEnumerable<IUncommittedEvent>, IEnumerable<IDomainEvent>> integrationEventMapper,
            Func<IEnumerable<IUncommittedEvent>, IEnumerable<IDomainEvent>>? internalEventMapper = null,
            CancellationToken token = default)
        {
            aggregateRoot.NotNull(nameof(aggregateRoot));
            var result = await beforeMessageProcess(aggregateRoot);
            if (result.Failed)
                return await GetResultAsync();
            result = await OnMessageProcessingAsync(aggregateRoot, integrationEventMapper, internalEventMapper, token);
            AddDomainResult(result);
            return await GetResultAsync();
        }

        protected virtual async Task<Result> ProcessMessageAsync(
            IAggregateRoot aggregateRoot,
            Func<IAggregateRoot, Task> afterMessageProcess,
            Func<IEnumerable<IUncommittedEvent>, IEnumerable<IDomainEvent>> integrationEventMapper,
            Func<IEnumerable<IUncommittedEvent>, IEnumerable<IDomainEvent>>? internalEventMapper = null,
            CancellationToken token = default)
        {
            aggregateRoot.NotNull(nameof(aggregateRoot));
            var result =
                await OnMessageProcessingAsync(aggregateRoot, integrationEventMapper, internalEventMapper, token);
            if (result.Failed)
                return await GetResultAsync();
            await afterMessageProcess(aggregateRoot);
            return await GetResultAsync();
        }

        protected virtual async Task<Result> ProcessMessageAsync(
            IAggregateRoot aggregateRoot,
            Func<IAggregateRoot, Task<Result>> beforeMessageProcess,
            Func<IAggregateRoot, Task> afterMessageProcess,
            Func<IEnumerable<IUncommittedEvent>, IEnumerable<IDomainEvent>> integrationEventMapper,
            Func<IEnumerable<IUncommittedEvent>, IEnumerable<IDomainEvent>>? internalEventMapper = null,
            CancellationToken token = default)
        {
            aggregateRoot.NotNull(nameof(aggregateRoot));
            var result = await beforeMessageProcess(aggregateRoot);
            if (result.Failed)
                return await GetResultAsync();
            result = await OnMessageProcessingAsync(aggregateRoot, integrationEventMapper, internalEventMapper, token);
            if (result.Failed)
                return await GetResultAsync();
            await afterMessageProcess(aggregateRoot);
            return await GetResultAsync();
        }

        protected void SetDomainMetadata(DomainMetadata domainMetadata)
            => DomainMetadata = domainMetadata;

        protected virtual void PrintMessage(object message, DomainMetadata domainMetadata)
        {
            var jsonSettings = Serializer.DefaultJsonSettings;
            jsonSettings.Formatting = Formatting.None;
            var logger = LoggerFactory.CreateLogger<MessageHandler<TMessage>>();

            logger.LogTrace(MessageTemplate.HandleMessage, MessageEventName.MessageHandler,
                typeof(MessageHandler<TMessage>).Name, message.GetType().GetGenericTypeName(), domainMetadata,
                message.Serialize(jsonSettings));
        }

        protected virtual void PrintFailed(object message, DomainMetadata domain,
            IEnumerable<ValidationFailure> failures)
        {
            PrintMessage(message, domain);
            var logger = LoggerFactory.CreateLogger<MessageHandler<TMessage>>();

            foreach (var failure in failures)
                logger.LogError("{Failure} \n}", failure);
        }

        protected virtual void IncreaseDomainRetry(DomainMetadata domain)
        {
            RetryCounter++;
            domain.IncreaseRetrying();
        }

        protected virtual void OnExecuteFailed()
        {
        }

        protected virtual IEnumerable<IDomainEvent> EmptyMapper(IEnumerable<IUncommittedEvent> arg)
            => Enumerable.Empty<IDomainEvent>();

        /// <summary></summary>
        /// <param name="message"></param>
        /// <returns></returns>
        protected Result Fail(string message) => Result.Fail(message);

        /// <summary></summary>
        /// <param name="message"></param>
        /// <param name="failures"></param>
        /// <returns></returns>
        protected Result Fail(string message, IEnumerable<ValidationFailure> failures) =>
            Result.Fail(message).WithValidationMessages(failures);

        /// <summary></summary>
        /// <returns></returns>
        protected Result Ok() => Result.Ok();

        /// <summary> </summary>
        /// <param name="event"></param>
        /// <param name="token"></param>
        /// <typeparam name="TDomainEvent"></typeparam>
        /// <returns></returns>
        protected virtual Task<Result> RaiseEventAsync<TDomainEvent>([NotNull] TDomainEvent @event,
            CancellationToken token = default) where TDomainEvent : class, IDomainEvent
            => DomainDispatcher.RaiseAsync(@event, typeof(TDomainEvent), DomainMetadata, token);

        /// <summary> </summary>
        /// <param name="command"></param>
        /// <param name="token"></param>
        /// <typeparam name="TCommand"></typeparam>
        /// <returns></returns>
        protected virtual Task<Result> SendCommandAsync<TCommand>(TCommand command,
            CancellationToken token = default) where TCommand : ICommand
            => DomainDispatcher.SendAsync(command, typeof(TCommand), DomainMetadata, token);

        /// <summary></summary>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        /// <typeparam name="TQuery"></typeparam>
        /// <returns></returns>
        protected virtual Task<TQuery?> QueryAsync<TQuery>(TQuery query, CancellationToken cancellationToken = default)
            where TQuery : IQuery<TQuery>
            => DomainDispatcher.QueryAsync(query, DomainMetadata, cancellationToken);

        protected virtual void AddDomainResult([NotNull] Result result)
        {
            if (result.Failed)
                DomainNotifier.SetDomainNotification(NotificationDomainEvent.From(result));
        }

        protected virtual void AddDomainError(MessageModel messageModel)
            => DomainNotifier.SetDomainNotification(NotificationDomainEvent.For(messageModel.Description));

        protected virtual void AddDomainValidation(ValidationFailure validationFailure, string description = "")
        {
            DomainNotifier.SetDomainNotification(
                NotificationDomainEvent.From(Result.Fail(description).WithValidationMessages(validationFailure)));
        }

        protected virtual void ExecuteFail(Result result)
        {
            var resultTemp = result.Clone();
            IgnoreErrors();
            OnExecuteFailed();
            throw BuildDomainException(resultTemp);
        }

        protected virtual Task ExecuteFailAsync([NotNull] Result result)
        {
            ExecuteFail(result);
            return Task.CompletedTask;
        }

        protected virtual void IgnoreErrors()
            => DomainNotifier.Reset();

        protected virtual Task<Result> GetResultAsync(MessageModel? messageModel = null)
            => ToResultAsync(messageModel);

        protected virtual Task<Result> ToResultAsync(MessageModel? messageModel)
        {
            var domainErrors = DomainNotifier.GetDomainNotification();

            if (!DomainNotifier.IsNotValid() && messageModel is null) return Task.FromResult(Result.Ok());

            var result = domainErrors.Result?.AddMessage(messageModel!)
                .WithMessageType(typeof(TMessage).Name) ?? Result.Fail("");

            switch (Message)
            {
                case IDomainEvent domainEvent:
                    result.WithMessageId(domainEvent.MessageId);
                    break;
                case ICommand command:
                    result.WithMessageId(command.MessageId);
                    break;
            }

            return Task.FromResult(result);
        }

        protected virtual DomainException BuildDomainException(Result? result)
            => result != null ? DomainException.For(result) : DomainException.Empty();
    }
}