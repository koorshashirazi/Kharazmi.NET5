#region

using System.Collections.Generic;
using System.Reflection;
using Kharazmi.Handlers;
using Microsoft.Extensions.DependencyInjection;

#endregion

namespace Kharazmi.BuilderExtensions
{
    internal static class HandlerExtensions
    {
        internal static IServiceCollection RegisterHandlers(this IServiceCollection services,
            Assembly[] handlerAssemblies)
        {
            List<Assembly> assemblies = new();
            assemblies.AddRange(handlerAssemblies);

            var entryAssembly = Assembly.GetEntryAssembly();

            if (entryAssembly is not null)
                assemblies.Add(entryAssembly);

            var assemblyCollection = assemblies.ToArray();

            services.ScanAssemblyFor(typeof(IEventHandler<>), assemblyCollection);
            services.ScanAssemblyFor(typeof(ICommandHandler<>), assemblyCollection);
            services.ScanAssemblyFor(typeof(IQueryHandler<,>), assemblyCollection);

            return services;
        }
    }
}