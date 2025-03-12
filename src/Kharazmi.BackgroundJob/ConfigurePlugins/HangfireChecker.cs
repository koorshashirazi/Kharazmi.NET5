using System.Linq;
using Kharazmi.Configuration;
using Kharazmi.ConfigurePlugins;
using Kharazmi.HealthChecks;
using Kharazmi.Options.Hangfire;

namespace Kharazmi.Hangfire.ConfigurePlugins
{
    internal class HangfireChecker : IHealthCheckerPlugin
    {
        public string CheckerName => nameof(HangfireChecker);

        public void HealthyCheck(IHealthChecker checker, ISettingProvider settingProvider)
        {
            var option = settingProvider.Get<HangfireOption>();
            if (option.Enable == false) return;

            var optionKeys = checker.GetUnHealthOptions<HangfireOption>().ToList();

            if (optionKeys.FirstOrDefault(x => x == nameof(HangfireOption)) != null)
            {
                option.Enable = false;
                settingProvider.UpdateOption(option);
            }

            if (optionKeys.FirstOrDefault(x => x == nameof(HangfireRedisStorageOption)) != null)
            {
                option.Enable = false;
                settingProvider.UpdateOption(option);
            }

            if (optionKeys.FirstOrDefault(x => x == nameof(HangfireMongoStorageOption)) != null)
            {
                option.Enable = false;
                settingProvider.UpdateOption(option);
            }
        }
    }
}