using Kharazmi.BuilderExtensions;

namespace Kharazmi.MongoDb.Cache
{
    public static class FrameworkBuilderExtensions
    {
        public static IConfigurePluginBuilder AddMongoRedisConfigurePlugin(this IConfigurePluginBuilder builder)
        {
            builder.AddConfigurePluginsFrom(new[] {typeof(AssemblyType).Assembly});
            return builder;
        }
    }
}