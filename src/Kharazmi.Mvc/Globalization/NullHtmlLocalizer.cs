using System.Collections.Generic;
using System.Linq;
using Kharazmi.Dependency;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;

namespace Kharazmi.Mvc.Globalization
{
    internal class NullHtmlLocalizer : IHtmlLocalizer, INullInstance
    {
        public LocalizedString GetString(string name)
            => new NullLocalizedString();

        public LocalizedString GetString(string name, params object[] arguments)
            => new NullLocalizedString();

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
            => Enumerable.Empty<LocalizedString>();

        public LocalizedHtmlString this[string name] => new LocalizedHtmlString("", "");

        public LocalizedHtmlString this[string name, params object[] arguments] => new LocalizedHtmlString("", "");
    }
}