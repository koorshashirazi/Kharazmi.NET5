using Kharazmi.Configuration;
using Kharazmi.HealthChecks;

namespace Kharazmi.ConfigurePlugins
{
    public interface IHealthCheckerPlugin
    {
        string CheckerName { get; }
        void HealthyCheck(IHealthChecker checker, ISettingProvider settingProvider);
    }
}