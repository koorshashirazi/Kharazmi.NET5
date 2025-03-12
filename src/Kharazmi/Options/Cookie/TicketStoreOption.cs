using System.Collections.Generic;
using Kharazmi.Constants;

namespace Kharazmi.Options.Cookie
{
    public class TicketStoreOption : ConfigurePluginOption
    {
        private readonly HashSet<string> _storageTypes;
        public TicketStoreOption()
        {
            StorageType = TicketStorageType.InMemory;
            _storageTypes = new HashSet<string> {TicketStorageType.InMemory, TicketStorageType.Distributed};
        }

        public string StorageType { get; set; }

        public IReadOnlyCollection<string> StorageTypes => _storageTypes;

        public override void Validate()
        {
        }

    }
}