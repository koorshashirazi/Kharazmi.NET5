using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration.Json;

namespace Kharazmi.Configuration
{
    internal class JsonSettingsProvider : JsonConfigurationProvider
    {
        public JsonSettingsProvider(JsonConfigurationSource source) : base(source)
        {
        }

        public void Reload()
        {
            Data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var file = Source.FileProvider?.GetFileInfo(Source.Path);
            if (file != null && file.Exists)
            {
                using var stream = file.CreateReadStream();
                Load(stream);
            }

            OnReload();
        }
    }
}