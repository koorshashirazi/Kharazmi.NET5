using System.Collections.Generic;
using Newtonsoft.Json;

namespace Kharazmi.Common.Events.Cache
{
    /// <summary>_</summary>
    public class CachesRemoved : DomainEvent
    {
       
        /// <summary>_</summary>
        [JsonConstructor]
        public CachesRemoved(IEnumerable<string> keys)
        {
            Keys = keys;
        }

        /// <summary>_</summary>
        public IEnumerable<string> Keys { get;  }
    }
}