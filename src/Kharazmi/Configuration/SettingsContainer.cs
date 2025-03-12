using Kharazmi.Dependency;

namespace Kharazmi.Configuration
{
    internal class SettingsContainer : Singleton<SettingsContainer>
    {
        private readonly FrameworkSettings _frameworkSettings;
        private readonly AppSettings _appSettings;

        private SettingsContainer()
        {
            _frameworkSettings = new();
            _appSettings = new();
        }

        public FrameworkSettings GetFrameworkSettings() => _frameworkSettings;
        public AppSettings GetAppSettings() => _appSettings;
    }
}