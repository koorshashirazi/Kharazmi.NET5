using System.Collections.Generic;
using Kharazmi.HealthChecks;
using Kharazmi.Options.HealthCheck;

namespace Kharazmi.Options
{
    public class PingOption : ConfigurePluginOption, IHaveHealthCheckOption
    {
        public PingOption()
        {
            HostHealthCheckOptions = new List<HostHealthCheckOption>();
        }

        public List<HostHealthCheckOption> HostHealthCheckOptions { get; set; }

        public bool UseHealthCheck { get; set; }
        public HealthCheckOption? HealthCheckOption { get; set; }

        public override void Validate()
        {
            if (UseHealthCheck == false) return;
            HealthCheckOption ??= new HealthCheckOption();
            HealthCheckOption.Validate();
        }
    }
}