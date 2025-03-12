using System.Reflection;
using Kharazmi.Common.Metadata;

namespace Kharazmi.Options
{
    public class ApplicationOption : ConfigurePluginOption
    {
        public ApplicationOption()
        {
            ApplicationMetadata = new MetadataCollection();
            Version = "1.0.0";
            Name = Assembly.GetEntryAssembly()?.GetName().Name;
        }
        public string? Title { get; set; }
        public string? Name { get; set; }
        public string Version { get; set; }

        public MetadataCollection ApplicationMetadata { get; set; }

        public override string ToString()
            => $"Title {Title}, Service: {Name}, Version {Version}";

        public override void Validate()
        {
        }
    }
}