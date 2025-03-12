using System;
using Microsoft.Extensions.DependencyInjection;

namespace Kharazmi.Dependency
{
    /// <summary>
    /// Service provider that allows for dynamic adding of new services
    /// </summary>
    public interface IDynamicServiceProvider : IServiceProvider
    {
        /// <summary>
        /// Add services to this collection
        /// </summary>
        IServiceCollection ServiceCollection { get; }
    }
}