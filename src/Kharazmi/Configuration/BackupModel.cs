namespace Kharazmi.Configuration
{
    public class BackupModel
    {
        public string? DefaultProvider { get; set; }
        public string? ProviderName { get; set; }
        public string[]? FileNames { get; set; }
    }
}