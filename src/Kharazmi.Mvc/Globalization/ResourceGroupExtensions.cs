using System.Linq;
using System.Text;
using Kharazmi.Extensions;
using Kharazmi.Json;
using Newtonsoft.Json;

namespace Kharazmi.Mvc.Globalization
{
    internal static class ResourceGroupExtensions
    {
        internal static string ToJavascript(this ResourceGroup resourceGroup)
        {
            var sb = new StringBuilder();
            var js = "resources";
            if (resourceGroup.JsVariableName.IsNotEmpty())
                js = resourceGroup.JsVariableName;
            
            sb.Append($"var {js} = ");

            var obj =
                resourceGroup.Entries.ToDictionary(x => x.Name.ToString(), x => x.Value);

            var setting = Serializer.DefaultJsonSettings;
            setting.Formatting = Formatting.None;
            var serialized = obj.Serialize(setting);

            sb.Append(serialized);
            sb.Append(";");
            // sb.Append("var SharedResource = JSON.parse(resources);");

            return sb.ToString();
        }
    }
}