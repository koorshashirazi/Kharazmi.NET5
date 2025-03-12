using System;
using System.Collections.Generic;
using Kharazmi.ConfigurePlugins;
using Microsoft.Extensions.DependencyInjection;

namespace Kharazmi
{
    public interface IConfigurePluginBuilder
    {
        event Action? PostInitializeConfigurePlugin;

        /// <summary> </summary>
        IServiceCollection Services { get; }

        IServiceCollection Build();

        IConfigurePluginBuilder AddPlugins(IEnumerable<IConfigurePlugin> plugins);
        
        IConfigurePluginBuilder AddBuilder<TBuilder>(TBuilder builder, bool releaseAfterBuild = true)
            where TBuilder : class;
        
        IConfigurePluginBuilder RemoveBuilder<TBuilder>(TBuilder builder)
            where TBuilder : class;

        T? GetBuilder<T>() where T : class;
        
    }
}