using System.Linq;
using Kharazmi.Configuration;
using Kharazmi.ConfigurePlugins;
using Kharazmi.HealthChecks;
using Kharazmi.Options;

namespace Kharazmi.Mvc.ConfigurePlugins
{
    public class EmailServerHealthCheckerPlugin : IHealthCheckerPlugin
    {
        public string CheckerName => nameof(EmailServerHealthCheckerPlugin);

        public void HealthyCheck(IHealthChecker checker, ISettingProvider settingProvider)
        {
            var option = settingProvider.Get<EmailServerOption>();
            if (option.Enable == false) return;

            var optionKeys = checker.GetUnHealthOptions<EmailServerOption>();
            if (optionKeys.FirstOrDefault(x => x == nameof(EmailServerOption)) == null) return;

            option.Enable = false;
            settingProvider.UpdateOption(option);
        }
    }
}