using Kharazmi.Options;
using Microsoft.AspNetCore.Mvc;

namespace Kharazmi.Mvc
{
    public class MvcOption : ConfigurePluginOption
    {
        public MvcOption()
        {
            MvcVersion = CompatibilityVersion.Latest;
            ResourcesPath = "Resources";
        }
        public CompatibilityVersion MvcVersion { get; set; }
        public bool ControllersAsServices { get; set; }
        public bool UseSessionStateTempData { get; set; }

        public bool UseLocalization { get; set; }
        public string ResourcesPath { get; set; }


        public override void Validate()
        {
        }
    }
}