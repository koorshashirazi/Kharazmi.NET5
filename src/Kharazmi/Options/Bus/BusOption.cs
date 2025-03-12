using System.Collections.Generic;
using Kharazmi.Constants;
using Kharazmi.Extensions;

namespace Kharazmi.Options.Bus
{
    public class BusOption : ConfigurePluginOption
    {
        private readonly HashSet<string> _busProviders;
        private string _defaultProvider;
        private bool _useSubscriber;
        private bool _useBusPublisher;
        private bool _useBusStorage;

        public BusOption()
        {
            _defaultProvider = BusProviderKey.RabbitMq;
            _busProviders = new HashSet<string>
            {
                BusProviderKey.RabbitMq,
                BusProviderKey.Redis
            };
        }


        public bool UseSubscriber
        {
            get => Enable && _useSubscriber && _defaultProvider.IsNotEmpty();
            set => _useSubscriber = value;
        }

        public bool UseBusPublisher
        {
            get => Enable && _useBusPublisher && _defaultProvider.IsNotEmpty();
            set => _useBusPublisher = value;
        }

        public string DefaultProvider
        {
            get => Enable ? _defaultProvider : BusProviderKey.Unknown;
            set
            {
                if (value.IsEmpty())
                {
                    Enable = false;
                    return;
                }

                if (!_busProviders.Contains(value))
                {
                    Enable = false;
                    return;
                }

                _defaultProvider = value;
            }
        }

        public IReadOnlyCollection<string> BusProviders => _busProviders;

        public bool UseBusStorage
        {
            get => Enable&& _useBusStorage;
            set => _useBusStorage = value;
        }

        public BusStoreOption? BusStoreOption { get; set; }

        public override void Validate()
        {
            if (!Enable) return;
            if (DefaultProvider.IsEmpty())
                AddValidation(MessageHelper.NullOrEmpty(MessageEventName.OptionsValidation, nameof(BusOption),
                    nameof(DefaultProvider)));

            if (UseBusStorage == false) return;
            BusStoreOption ??= new BusStoreOption();
            BusStoreOption.Validate();
        }
    }
}