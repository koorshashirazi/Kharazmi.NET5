using Microsoft.AspNetCore.Builder;

namespace Kharazmi.Localization
{
    public static class MongoInstallerExtensions
    {
        public static IApplicationBuilder UseMongoDbInstaller(this IApplicationBuilder appBuilder)
        {
          

            // var installers = service.GetServices<IMongoDbInstaller>();
            return appBuilder.UseMiddleware<MongoDbInstallerMiddleware>();
        }
    }
}