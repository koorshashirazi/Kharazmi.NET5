using System.Collections.Generic;
using System.Linq;
using Kharazmi.Constants;
using Kharazmi.Extensions;
using Kharazmi.Options.Cookie;

namespace Kharazmi.Options
{
    public sealed class CacheOption : ConfigurePluginOption
    {
        private bool _useNotificationStore;
        private string _distributedCacheManager;
        private string _cacheManagerType;
        private bool _useInMemoryCache;
        private bool _useDistributedCache;

        public CacheOption()
        {
            ExpirationOption = new ExpirationOption();
            _cacheManagerType = CacheManagerTypeConstants.ModeLess;
            _distributedCacheManager = DistributedCacheManagerTypeConstants.ModeLess;
        }

        public bool UseInMemoryCache
        {
            get => Enable && _useInMemoryCache;
            set => _useInMemoryCache = value;
        }

        public bool UseInMemoryNotificationStore
        {
            get => _useInMemoryCache && _useNotificationStore;
            set => _useNotificationStore = value;
        }

        public string CacheManagerType
        {
            get => _useInMemoryCache ? _cacheManagerType : CacheManagerTypeConstants.ModeLess;
            set
            {
                if (CacheManagerTypes.Contains(value))
                    _cacheManagerType = value;
            }
        }

        public IReadOnlyCollection<string> CacheManagerTypes => CacheManagerTypeConstants.Providers;

        public string? CacheProviderOptionKey { get; set; }

        public bool UseDistributedCache
        {
            get => Enable && _useDistributedCache;
            set => _useDistributedCache = value;
        }

        public string DistributedCacheManagerType
        {
            get => _useDistributedCache ? _distributedCacheManager : DistributedCacheManagerTypeConstants.ModeLess;
            set
            {
                if (DistributedCacheManagerTypes.Contains(value))
                    _distributedCacheManager = value;
            }
        }

        public IReadOnlyCollection<string> DistributedCacheManagerTypes =>
            DistributedCacheManagerTypeConstants.Providers;

        public string? DistributedProviderOptionKey { get; set; }
        public ExpirationOption ExpirationOption { get; set; }

        public override void Validate()
        {
            ExpirationOption.Validate();

            if (CacheManagerType == CacheManagerTypeConstants.RedisDb &&
                CacheProviderOptionKey.IsEmpty())
                AddValidation(MessageHelper.NullOrEmpty(MessageEventName.OptionsValidation,
                    nameof(CacheOption), nameof(CacheManagerType)));

            if ((DistributedCacheManagerType == DistributedCacheManagerTypeConstants.RedisDb ||
                 DistributedCacheManagerType == DistributedCacheManagerTypeConstants.MongoDb) &&
                DistributedProviderOptionKey.IsEmpty())
                AddValidation(MessageHelper.NullOrEmpty(MessageEventName.OptionsValidation,
                    nameof(CacheOption), nameof(DistributedCacheManagerType)));
        }
    }
}