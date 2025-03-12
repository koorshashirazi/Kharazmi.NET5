using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Kharazmi.Extensions;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Kharazmi.Mvc.Globalization
{
    [HtmlTargetElement("resources")]
    public class ResourcesTagHelper : TagHelper
    {
        private readonly IHtmlLocalizerFactory? _factory;

        public ResourcesTagHelper([AllowNull] IHtmlLocalizerFactory factory)
        {
            _factory = factory;
        }


        public bool OnContentLoaded { get; set; } = false;
        public string? ResourceType { get; set; }

        public string? JsVariableName { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (OnContentLoaded)
            {
                await base.ProcessAsync(context, output);
            }
            else
            {
                if (ResourceType.IsEmpty()) return;
                var resourceType = Type.GetType(ResourceType);
                if (resourceType is null) return;

                var resourceTypeInfo = resourceType.GetTypeInfo();

                var fullName = resourceTypeInfo.Assembly.FullName;
                if (string.IsNullOrEmpty(fullName))
                {
                    await base.ProcessAsync(context, output);
                    return;
                }

                var assemblyName = new AssemblyName(fullName);
                var localizer = _factory?.Create(resourceTypeInfo.Name, assemblyName.FullName);

                if (localizer is null) return;
                try
                {
                    var entries = localizer.GetAllStrings(true).ToList();

                    var groupedResources =
                        new ResourceGroup(resourceTypeInfo.Name, entries, JsVariableName);

                    var sb = new StringBuilder();
                    sb.Append(groupedResources.ToJavascript());

                    var content = await output.GetChildContentAsync();
                    sb.Append(content.GetContent());

                    output.TagName = "script";
                    output.Content.AppendHtml(sb.ToString());
                }
                catch (Exception)
                {
                    await base.ProcessAsync(context, output);
                }
            }
        }
    }
}