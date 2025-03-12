using Kharazmi.Dependency;
using Microsoft.Extensions.Localization;

namespace Kharazmi.Mvc.Globalization
{
    internal class NullLocalizedString : LocalizedString, INullInstance
    {
        public NullLocalizedString() : this("", "")
        {
        }

        public NullLocalizedString(string name, string value) : base(name, value)
        {
        }

        public NullLocalizedString(string name, string value, bool resourceNotFound) : base(name, value,
            resourceNotFound)
        {
        }

        public NullLocalizedString(string name, string value, bool resourceNotFound, string? searchedLocation) : base(
            name, value, resourceNotFound, searchedLocation)
        {
        }
    }
}