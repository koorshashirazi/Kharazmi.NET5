#region

using Kharazmi.BuilderExtensions;

#endregion

namespace Kharazmi.Localization.Extensions
{
    public static class FrameworkBuilderExtensions
    {
        public static IConfigurePluginBuilder AddMongoConfigurePlugin(this IConfigurePluginBuilder builder)
        {
            MongoMapper.Initial();
            builder.AddConfigurePluginsFrom(new[] {typeof(AssemblyType).Assembly});
            return builder;
        }
    }
}