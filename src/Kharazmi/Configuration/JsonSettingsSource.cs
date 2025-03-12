using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace Kharazmi.Configuration
{
    internal class JsonSettingsSource : JsonConfigurationSource
    {
        public override IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            EnsureDefaults(builder);
            Optional = false;
            ReloadOnChange = true;
            return new JsonSettingsProvider(this);
        }
    }
}