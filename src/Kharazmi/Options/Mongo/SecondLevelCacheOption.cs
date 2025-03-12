using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Kharazmi.Constants;
using Kharazmi.Extensions;

namespace Kharazmi.Options.Mongo
{
    public class SecondLevelCacheOption : NestedOption
    {
        private string _cacheProvider;

        public SecondLevelCacheOption()
        {
            _cacheProvider = CacheManagerTypeConstants.ModeLess;
        }

        public string CacheProvider
        {
            get => _cacheProvider;
            set
            {
                if (CacheProviders.Contains(_cacheProvider))
                    _cacheProvider = value;
            }
        }

        public IReadOnlyCollection<string> CacheProviders => CacheManagerTypeConstants.Providers;

        [StringLength(100)] public string? CacheProviderOptionKey { get; set; }

        public override void Validate()
        {

            if (CacheProviderOptionKey.IsEmpty())
                AddValidation(MessageHelper.NullOrEmpty(MessageEventName.OptionsValidation,
                    nameof(SecondLevelCacheOption), nameof(CacheProviderOptionKey)));

            if (CacheProvider.IsEmpty())
                AddValidation(MessageHelper.NullOrEmpty(MessageEventName.OptionsValidation,
                    nameof(SecondLevelCacheOption), nameof(CacheProvider)));

            if (!CacheProviders.Contains(CacheProvider))
                AddValidation(MessageHelper.NotFoundValueInCollection(MessageEventName.OptionsValidation,
                    nameof(SecondLevelCacheOption), nameof(CacheProviders), CacheProvider));
        }
    }
}