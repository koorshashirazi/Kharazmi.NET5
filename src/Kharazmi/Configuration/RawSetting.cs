using System.ComponentModel.DataAnnotations;

namespace Kharazmi.Configuration
{
    public class RawSetting
    {
        public string? JsonData { get; set; }
        
        [StringLength(100)]
        public string? ProviderName { get; set; }
    }
}