namespace Kharazmi.Options
{
    public interface IConfigurePluginOption : IOptions
    {
    }

    public abstract class ConfigurePluginOption : Options, IConfigurePluginOption
    {
        protected ConfigurePluginOption()
        {
        }
    }
}