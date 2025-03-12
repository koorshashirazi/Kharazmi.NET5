#region

using Kharazmi.Dependency;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;

#endregion

namespace Kharazmi.Mvc.Globalization
{
    public interface IResourceManager<TResource> : IShouldBeSingleton
        where TResource : class, IResourceType
    {
        IStringLocalizerFactory StringFactory { get; }
        IHtmlLocalizerFactory HtmlFactory { get; }
        IHtmlLocalizer Html { get; }
        IStringLocalizer Strings { get; }
        string this[string? key] { get; }
        LocalizedString GetValue(string? key);
        LocalizedString GetValueTitle(string? key);
        string GetValueFormatter(string? key, params object[] parameter);
        LocalizedHtmlString GetValue(string? key, params object[] parameter);
    }
}