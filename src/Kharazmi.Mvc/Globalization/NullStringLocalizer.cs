using System.Collections.Generic;
using System.Linq;
using Kharazmi.Dependency;
using Microsoft.Extensions.Localization;

namespace Kharazmi.Mvc.Globalization
{
    internal class NullStringLocalizer : IStringLocalizer, INullInstance
    {
        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
            => Enumerable.Empty<LocalizedString>();

        public LocalizedString this[string name] => new NullLocalizedString();

        public LocalizedString this[string name, params object[] arguments] => new NullLocalizedString();
    }
}