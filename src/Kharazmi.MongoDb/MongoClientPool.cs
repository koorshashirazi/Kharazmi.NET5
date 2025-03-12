using System;
using System.Collections.Concurrent;
using Kharazmi.Configuration;
using Kharazmi.Constants;
using Kharazmi.Localization.Extensions;
using Kharazmi.Options;
using Kharazmi.Options.Mongo;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Kharazmi.Localization
{
    internal class MongoClientPool : IMongoClientPool
    {
        private readonly ILogger<MongoClientPool>? _logger;
        private readonly ConcurrentDictionary<string, MongoClient> _clients = new();

        public MongoClientPool(ISettingProvider settings, ILogger<MongoClientPool>? logger)
        {
            settings.UpdatedOptionHandler += OnUpdatedOptionHandler;
            _logger = logger;
        }

        public MongoClient Client(MongoOption option)
            => _clients.GetOrAdd(option.OptionKey, type => new MongoClient(option.BuildClientSettings()));

        private void OnUpdatedOptionHandler(ISettingProvider sender, IOptions options, Type optionType)
        {
            if (!(options is MongoOptions mongoOptions)) return;

            foreach (var option in mongoOptions.ChildOptions)
            {
                if (!option.IsDirty) continue;
                _logger?.LogTrace(MessageTemplate.OptionChanged, MessageEventName.OptionChanged,
                    nameof(MongoClientPool), option.GetType().Name);
                _clients.TryRemove(option.OptionKey, out _);
            }
        }
    }
}