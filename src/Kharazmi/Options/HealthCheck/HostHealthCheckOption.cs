using System.ComponentModel.DataAnnotations;

namespace Kharazmi.Options.HealthCheck
{
    public class HostHealthCheckOption: NestedOption
    {
        [StringLength(100)] public string? Host { get; set; }
        public int Timeout { get; set; }
        public override void Validate()
        {
            
        }
    }
}