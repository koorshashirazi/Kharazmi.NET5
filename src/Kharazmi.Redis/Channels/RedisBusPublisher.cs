using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Kharazmi.Background;
using Kharazmi.Channels;
using Kharazmi.Common.Events;
using Kharazmi.Common.Metadata;
using Kharazmi.Configuration;
using Kharazmi.Dependency;
using Kharazmi.Extensions;
using Kharazmi.Functional;
using Kharazmi.Json;
using Kharazmi.Options;
using Kharazmi.Redis.Extensions;
using Kharazmi.Redis.Jobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Kharazmi.Redis.Channels
{
    internal class RedisBusPublisher : IChannelPublisher
    {
        private readonly IBackgroundService _background;
        private readonly IPubSubFactory _pubSubFactory;
        private readonly ISettingProvider _settingProvider;
        private readonly ILoggerFactory? _loggerFactory;

        /// <summary> </summary>
        public RedisBusPublisher(
            ServiceFactory<IBackgroundService> backgroundFactory,
            ServiceFactory<IPubSubFactory> factory,
            ISettingProvider settingProvider,
            [AllowNull] ILoggerFactory? loggerFactory)
        {
            _pubSubFactory = factory.Instance();
            _background = backgroundFactory.Instance();
            _settingProvider = settingProvider;
            _loggerFactory = loggerFactory;
        }


        public async Task<Result> PublishAsync<TEvent>(
            [NotNull] TEvent domainEvent,
            MetadataCollection? metadata = null,
            bool? publishAsynchronous = null,
            CancellationToken token = default) where TEvent : class, IDomainEvent
        {
            var domainMetadata = DomainMetadata.Empty;
            domainMetadata.AddRange(metadata);
            var logger = _loggerFactory?.CreateLogger<RedisBusPublisher>();

            try
            {
                var redisDbOption = _settingProvider.GetRedisOption(domainMetadata.BusOptionKey);

                var isAsynchronous = publishAsynchronous ?? redisDbOption.PublishAsynchronous;
                if (isAsynchronous)
                {
                    var domainEventJob = CreateJob(redisDbOption.OptionKey, redisDbOption, domainEvent,
                        domainMetadata);
                    var jobId = await _background.EnqueueJobAsync(domainEventJob).ConfigureAwait(false);

                    return Result.Ok(jobId);
                }

                var channelLogger = _loggerFactory?.CreateLogger<PublishDomainEventJob>() ??
                                    NullLogger<PublishDomainEventJob>.Instance;

                var result = await CreateJob(redisDbOption.OptionKey, redisDbOption, domainEvent, domainMetadata)
                    .TryExecuteAsync(_pubSubFactory, _settingProvider, channelLogger);

                if (result.Failed && domainEvent.IsEssentials)
                    return result;

                return result;
            }
            catch (Exception e)
            {
                logger?.LogError("{Message}", e.Message);
                return Result.Fail(e.Message);
            }
        }

        private static PublishDomainEventJob CreateJob<TEvent>(
            string optionKey,
            IChannelOptions redisDbOptions,
            TEvent domainEvent,
            MetadataCollection metadata)
            where TEvent : class, IDomainEvent
        {
            var channel = domainEvent.ChannelName(redisDbOptions.ChannelPrefix);
            return new(new DomainEventSerialized(
                optionKey,
                domainEvent.MessageAssemblyType!,
                channel.DomainMessageMetadata.ChannelName!,
                domainEvent.Serialize(),
                metadata.Serialize(),
                (int) domainEvent.DomainMessageMetadata.ChannelCommandFlags))
            {
                RetryOption = redisDbOptions.RetryOption
            };
        }
    }
}