using System.Linq;
using Kharazmi.Configuration;
using Kharazmi.ConfigurePlugins;
using Kharazmi.HealthChecks;
using Kharazmi.Options.Redis;

namespace Kharazmi.Redis.ConfigurePlugins
{
    internal class RedisHealthCheckerPlugin : IHealthCheckerPlugin
    {
        public string CheckerName => nameof(RedisHealthCheckerPlugin);

        public void HealthyCheck(IHealthChecker checker, ISettingProvider settingProvider)
        {
            var option = settingProvider.Get<RedisDbOptions>();
            if(option.Enable == false) return;

            var optionKeys = checker.GetUnHealthOptions<RedisDbOptions>().ToList();

            foreach (var optionKey in optionKeys)
            {
                var redisDbOption = option.FindOrNone(x => x.OptionKey == optionKey);
                if (redisDbOption is null) continue;
                redisDbOption.Enable = false;
                option.UpdateChildOption(redisDbOption);
            }

            if (optionKeys.FirstOrDefault(x => x == nameof(RedisDbOptions)) == null)
                return;
            
            option.Enable = false;
            settingProvider.UpdateOption(option);
        }
    }
}