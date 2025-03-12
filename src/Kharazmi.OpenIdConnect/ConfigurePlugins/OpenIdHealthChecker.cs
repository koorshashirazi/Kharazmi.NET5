using System.Linq;
using Kharazmi.Configuration;
using Kharazmi.ConfigurePlugins;
using Kharazmi.HealthChecks;
using Kharazmi.Options.Cookie;

namespace Kharazmi.OpenIdConnect.ConfigurePlugins
{
    internal class OpenIdHealthChecker : IHealthCheckerPlugin
    {
        public string CheckerName => nameof(OpenIdHealthChecker);

        public void HealthyCheck(IHealthChecker checker, ISettingProvider settingProvider)
        {
            var cookieOption = settingProvider.Get<ExtendedCookieOption>();
            if (cookieOption.Enable == false) return;

            cookieOption.Validate();
            if (cookieOption.IsValid() == false)
            {
                cookieOption.Enable = false;
                settingProvider.UpdateOption(cookieOption);
            }

            var openIdOption = settingProvider.Get<ExtendedOpenIdOption>();
            if (openIdOption.Enable == false) return;

            var optionKeys = checker.GetUnHealthOptions<ExtendedOpenIdOption>();
            if (optionKeys.FirstOrDefault(x => x == nameof(ExtendedOpenIdOption)) == null) return;

            openIdOption.Enable = false;
            settingProvider.UpdateOption(openIdOption);
        }
    }
}