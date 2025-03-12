using System.Linq;
using Kharazmi.Configuration;
using Kharazmi.ConfigurePlugins;
using Kharazmi.HealthChecks;
using Kharazmi.Options.RabbitMq;

namespace Kharazmi.RabbitMq.ConfigurePlugins
{
    internal class RabbitMqHealthCheckerPlugin : IHealthCheckerPlugin
    {
        public string CheckerName => nameof(RabbitMqHealthCheckerPlugin);

        public void HealthyCheck(IHealthChecker checker, ISettingProvider settingProvider)
        {
            var option = settingProvider.Get<RabbitMqOption>();
            if(option.Enable == false) return;

            var optionKeys = checker.GetUnHealthOptions<RabbitMqOption>();
            if ( optionKeys.FirstOrDefault(x => x == nameof(RabbitMqOption)) == null) return;

            option.Enable = false;
            settingProvider.UpdateOption(option);
        }
    }
}