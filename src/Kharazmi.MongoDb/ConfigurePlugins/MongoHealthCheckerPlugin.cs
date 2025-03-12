using System.Linq;
using Kharazmi.Configuration;
using Kharazmi.ConfigurePlugins;
using Kharazmi.HealthChecks;
using Kharazmi.Options.Mongo;

namespace Kharazmi.Localization.ConfigurePlugins
{
    internal class MongoHealthCheckerPlugin : IHealthCheckerPlugin
    {
        public string CheckerName => nameof(MongoHealthCheckerPlugin);

        public void HealthyCheck(IHealthChecker checker, ISettingProvider settingProvider)
        {
            var option = settingProvider.Get<MongoOptions>();
            if(option.Enable == false) return;

            var optionKeys = checker.GetUnHealthOptions<MongoOptions>().ToList();

            foreach (var optionKey in optionKeys)
            {
                var mongoOption = option.FindOrNone(x => x.OptionKey == optionKey);
                if (mongoOption is null) continue;
                mongoOption.Enable = false;
                option.UpdateChildOption(mongoOption);
            }

            if (optionKeys.FirstOrDefault(x => x == nameof(MongoOptions)) == null) return;

            option.Enable = false;
            settingProvider.UpdateOption(option);
        }
    }
}