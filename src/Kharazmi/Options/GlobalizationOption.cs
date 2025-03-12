#region

using System;
using System.Collections.Generic;
using Kharazmi.Constants;
using Kharazmi.Extensions;
using Newtonsoft.Json;

#endregion

namespace Kharazmi.Options
{
    public class GlobalizationOption : ConfigurePluginOption
    {
        public GlobalizationOption()
        {
            SupportedCultures = new HashSet<string> {"en"};
            DefaultCultureCookieExpires = TimeSpan.FromDays(30);
            DefaultSupportedCulture = "en";
        }

        public bool UseResourceManager { get; set; }
        public string? CookieName { get; set; }

        public string? DefaultSupportedCulture { get; set; }

        public HashSet<string> SupportedCultures { get; set; }

        public string? DateFormatString { get; set; }

        [JsonProperty(TypeNameHandling = TypeNameHandling.All)]
        public TimeSpan DefaultCultureCookieExpires { get; set; }


        public override void Validate()
        {
            if (CookieName.IsEmpty())
                AddValidation(MessageHelper.NullOrEmpty(MessageEventName.OptionsValidation,
                    nameof(GlobalizationOption), nameof(CookieName)));
        }
    }
}