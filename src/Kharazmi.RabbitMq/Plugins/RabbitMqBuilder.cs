using System;
using System.Collections.Generic;
using Kharazmi.Options.RabbitMq;
using RawRabbit.DependencyInjection;
using RawRabbit.Instantiation;

namespace Kharazmi.RabbitMq.Plugins
{
    /// <summary>_</summary>
    public sealed class RabbitMqBuilder
    {
        /// <summary>_</summary>
        public IConfigurePluginBuilder Builder { get; }

        private static readonly List<Action<RabbitMqOption, IClientBuilder>> _plugins = new();
        private static readonly List<Action<IServiceProvider, IDependencyRegister>> _dependency = new();

        /// <summary>_</summary>
        public RabbitMqBuilder(IConfigurePluginBuilder builder)
        {
            Builder = builder;
        }


        /// <summary>_</summary>
        public RabbitMqBuilder AddDependency(Action<IServiceProvider, IDependencyRegister> dependencies)
        {
            _dependency.Add(dependencies);
            return this;
        }

        /// <summary>_</summary>
        public RabbitMqBuilder AddPlugin(Action<RabbitMqOption, IClientBuilder> buildClient)
        {
            _plugins.Add(buildClient);
            return this;
        }

        /// <summary>_</summary>
        public IConfigurePluginBuilder BuildDependencies(IServiceProvider sp, IDependencyRegister ioc)
        {
            _dependency.ForEach(t => t.Invoke(sp, ioc));
            return Builder;
        }

        /// <summary>_</summary>
        public IConfigurePluginBuilder BuildPlugins(RabbitMqOption option, IClientBuilder builder)
        {
            _plugins.ForEach(t => t.Invoke(option, builder));
            return Builder;
        }
    }
}