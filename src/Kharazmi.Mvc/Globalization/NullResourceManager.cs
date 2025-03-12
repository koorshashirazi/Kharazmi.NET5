using Kharazmi.Dependency;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;

namespace Kharazmi.Mvc.Globalization
{
    internal class NullResourceManager<TResource> : IResourceManager<TResource>, INullInstance
        where TResource : class, IResourceType
    {
        public NullResourceManager()
        {
            Html = new NullHtmlLocalizer();
            Strings = new NullStringLocalizer();
            StringFactory = new NullStringLocalizerFactory();
            HtmlFactory = new NullHtmlLocalizerFactory();
        }

        public IStringLocalizerFactory StringFactory { get; }
        public IHtmlLocalizerFactory HtmlFactory { get; }
        public IHtmlLocalizer? Html { get; }
        public IStringLocalizer? Strings { get; }

        public string this[string? key] => "";

        public LocalizedString GetValue(string? key)
            => new NullLocalizedString();

        public LocalizedString GetValueTitle(string? key)
            => new NullLocalizedString();

        public string GetValueFormatter(string? key, params object[] parameter)
            => "";

        public LocalizedHtmlString GetValue(string? key, params object[] parameter)
            => new("", "");
    }
}