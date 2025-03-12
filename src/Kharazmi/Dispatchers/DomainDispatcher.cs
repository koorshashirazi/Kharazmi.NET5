#region

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Background;
using Kharazmi.Common.Metadata;
using Kharazmi.Configuration;
using Kharazmi.Constants;
using Kharazmi.Dependency;
using Kharazmi.Domain;
using Kharazmi.Functional;
using Kharazmi.Jobs;
using Kharazmi.Json;
using Kharazmi.Messages;
using Kharazmi.Options.Domain;
using Microsoft.Extensions.Logging;

#endregion

namespace Kharazmi.Dispatchers
{
    internal class DomainDispatcher : IDomainDispatcher
    {
        private readonly IBackgroundService _background;
        private readonly ISettingProvider _settingProvider;
        private readonly IDomainMetadataAccessor _domainMetadataAccessor;
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly IQueryDispatcher _queryDispatcher;
        private readonly ILogger<DomainDispatcher> _logger;

        public DomainDispatcher(
            ISettingProvider settingProvider,
            ServiceFactory<IBackgroundService> background,
            ServiceFactory<ICommandDispatcher> commandDispatcher,
            ServiceFactory<IEventDispatcher> eventDispatcher,
            ServiceFactory<IQueryDispatcher> queryDispatcher,
            ServiceFactory<IDomainMetadataAccessor> domainMetadataAccessor)
        {
            _background = background.Instance();
            _settingProvider = settingProvider;
            _domainMetadataAccessor = domainMetadataAccessor.Instance();
            _commandDispatcher = commandDispatcher.Instance();
            _eventDispatcher = eventDispatcher.Instance();
            _queryDispatcher = queryDispatcher.Instance();
            _logger = background.LoggerFactory.CreateLogger<DomainDispatcher>();
        }

        public Task<Result> SendAsync(object? command, Type commandType, MetadataCollection? metadata = null,
            CancellationToken token = default)
        {
            var isAsynchronous = _settingProvider.Get<DomainOption>().DispatchAsynchronous;
            return SendAsync(command, commandType, isAsynchronous, metadata, token);
        }

        public Task<Result> RaiseAsync(object? domainEvent, Type eventType, MetadataCollection? metadata = null,
            CancellationToken token = default)
        {
            var isAsynchronous = _settingProvider.Get<DomainOption>().DispatchAsynchronous;
            return RaiseAsync(domainEvent, eventType, isAsynchronous, metadata, token);
        }


        public Task<TResult?> QueryAsync<TResult>([NotNull] IQuery<TResult> query, MetadataCollection? metadata = null,
            CancellationToken token = default)
        {
            try
            {
                metadata ??= _domainMetadataAccessor.DomainMetadata;
                return _queryDispatcher.QueryAsync(query, metadata, token);
            }
            catch (Exception e)
            {
                _logger.LogError(MessageTemplate.DomainDispatcherSendMessageFailed, MessageEventName.DomainDispatcher,
                    nameof(DomainDispatcher), query.GetType().FullName, e.Message);

                return Task.FromResult<TResult>(default!)!;
            }
        }

        public async Task<Result> SendAsync(object? command, Type commandType, bool dispatchAsynchronous = false,
            MetadataCollection? metadata = null, CancellationToken token = default)
        {
            try
            {
                if (command is null)
                    return Result.Fail("Invalid command object or command type, Domain dispatcher can't send command");

                metadata ??= _domainMetadataAccessor.DomainMetadata;

                if (!dispatchAsynchronous)
                    return await _commandDispatcher.SendAsync(command, commandType, metadata, token);

                var job = new SendCommandJob(new MessageSerialized(
                    commandType.AssemblyQualifiedName,
                    command.Serialize(),
                    metadata.Serialize()));

                var jobId = await _background.EnqueueJobAsync(job);

                _logger.LogInformation(MessageTemplate.DomainDispatcherSendMessage, MessageEventName.DomainDispatcher,
                    nameof(DomainDispatcher), commandType.FullName);

                return Result.Ok(jobId);
            }
            catch (Exception e)
            {
                _logger.LogError(MessageTemplate.DomainDispatcherSendMessageFailed, MessageEventName.DomainDispatcher,
                    nameof(DomainDispatcher), commandType.FullName, e.Message);

                return Result.Fail($"Send a domain message was failed, with messageType: {commandType.FullName}");
            }
        }

        public async Task<Result> RaiseAsync(object? domainEvent, Type eventType, bool dispatchAsynchronous = false,
            MetadataCollection? metadata = null, CancellationToken token = default)
        {
            try
            {
                if (domainEvent is null)
                    return Result.Fail(
                        "Invalid domain event object or domain event type, Domain dispatcher can't send event");

                metadata ??= _domainMetadataAccessor.DomainMetadata;

                if (!dispatchAsynchronous)
                    return await _eventDispatcher.RaiseAsync(domainEvent, eventType, metadata, token)
                        .ConfigureAwait(false);

                var job = new RaiseDomainEventJob(new MessageSerialized(
                    eventType.AssemblyQualifiedName,
                    domainEvent.Serialize(),
                    metadata.Serialize()));

                var jobId = await _background.EnqueueJobAsync(job);

                _logger.LogInformation(MessageTemplate.DomainDispatcherSendMessage, MessageEventName.DomainDispatcher,
                    nameof(DomainDispatcher), eventType.FullName);

                return Result.Ok(jobId);
            }
            catch (Exception e)
            {
                _logger.LogError(MessageTemplate.DomainDispatcherSendMessageFailed, MessageEventName.DomainDispatcher,
                    nameof(DomainDispatcher), eventType.FullName, e.Message);

                return Result.Fail($"Send a domain message was failed, with messageType: {eventType.FullName}");
            }
        }
    }
}