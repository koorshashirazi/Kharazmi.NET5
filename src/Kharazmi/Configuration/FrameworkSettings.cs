namespace Kharazmi.Configuration
{
    public interface IFrameworkSettings : ISettings
    {
    }

    /// <summary>_</summary>
    public sealed class FrameworkSettings : Settings, IFrameworkSettings
    {
        /// <summary>_</summary>
        public FrameworkSettings()
        {
        }
    }

    public interface IAppSettings : ISettings
    {
    }

    /// <summary>_</summary>
    public sealed class AppSettings : Settings, IAppSettings
    {
        /// <summary>_</summary>
        public AppSettings()
        {
        }
    }
}