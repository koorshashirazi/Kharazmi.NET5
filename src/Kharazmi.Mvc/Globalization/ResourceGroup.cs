using System.Collections.Generic;
using Microsoft.Extensions.Localization;

namespace Kharazmi.Mvc.Globalization
{
    internal class ResourceGroup
    {
        public ResourceGroup(string name, IEnumerable<LocalizedString> entries, string? jsVariableName)
        {
            Name = name;
            Entries = entries;
            JsVariableName = jsVariableName;
        }

        public string Name { get; }
        public string? JsVariableName { get;  }
        public IEnumerable<LocalizedString> Entries { get; }
        
    }
}