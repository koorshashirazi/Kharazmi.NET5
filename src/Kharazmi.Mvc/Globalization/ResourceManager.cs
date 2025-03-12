#region

using System.Reflection;
using Kharazmi.Extensions;
using Kharazmi.Guard;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;

#endregion

namespace Kharazmi.Mvc.Globalization
{
    internal class ResourceManager<TResource> : IResourceManager<TResource> where TResource : class, IResourceType
    {
        public IStringLocalizerFactory StringFactory { get; }
        public IHtmlLocalizerFactory HtmlFactory { get; }
        public IHtmlLocalizer Html { get; }
        public IStringLocalizer? Strings { get; }

        public ResourceManager(
            IStringLocalizerFactory factory,
            IHtmlLocalizerFactory htmlLocalizerFactory)
        {
            StringFactory = factory;
            HtmlFactory = htmlLocalizerFactory;
            factory.NotNull(nameof(factory));
            htmlLocalizerFactory.NotNull(nameof(htmlLocalizerFactory));

            var resourceType = typeof(TResource);
            var assemblyName = new AssemblyName(resourceType.GetTypeInfo().Assembly.FullName!);
            
            Strings = factory.Create(resourceType.Name, assemblyName.FullName);
            Html = htmlLocalizerFactory.Create(resourceType.Name, assemblyName.FullName);
        }

        public string this[string? key] => key.IsEmpty()?  string.Empty :  Strings?[key] ?? string.Empty;

        public LocalizedString GetValue(string? key)
        {
            return (!string.IsNullOrWhiteSpace(key)  ? Strings?[key] : new LocalizedString("Empty_key", "Empty_Value")) ??
                   new LocalizedString("Empty_key", "Empty_Value");
        }

        public LocalizedHtmlString GetValue(string? key, params object[] parameter)
            => Html?[key, parameter] ?? new LocalizedHtmlString("Empty_key", "Empty_Value");

        public string GetValueFormatter(string? key, params object[] parameter)
        {
            return key.IsNotEmpty()
                ? string.Format(GetValue(key).ToString(), parameter)
                : string.Format(GetValue(key).ToString());
        }

        public LocalizedString GetValueTitle(string? key)
            => GetValue($"{key}Title");
    }
}