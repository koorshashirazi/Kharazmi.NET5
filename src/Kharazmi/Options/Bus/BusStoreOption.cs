using System;
using System.Collections.Generic;
using Kharazmi.Bus;
using Kharazmi.Constants;
using Kharazmi.Extensions;

namespace Kharazmi.Options.Bus
{
    public class BusStoreOption : NestedOption
    {
        public BusStoreOption()
        {
            BusMessageStoreType = BusMessageStoreTypeConstants.None;
            BusMessageStoredScheme = BusMessageStored.DefaultBusMessageStoredScheme;
            ExpireAt = TimeSpan.Zero;
        }

        public string BusMessageStoreType { get; set; }
        public IReadOnlyCollection<string> BusMessageStoreTypes => BusMessageStoreTypeConstants.GetStorageType();

        public string BusMessageStoredScheme { get; set; }

        public TimeSpan ExpireAt { get; set; }

        public override void Validate()
        {
            if (BusMessageStoreType.IsEmpty())
                BusMessageStoreType = BusMessageStoreTypeConstants.None;

            if (BusMessageStoredScheme.IsEmpty())
                BusMessageStoredScheme = BusMessageStored.DefaultBusMessageStoredScheme;
        }
    }
}