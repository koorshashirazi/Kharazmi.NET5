using System;
using Microsoft.Extensions.DependencyInjection;

namespace Kharazmi
{
    internal readonly struct ConfigurePluginBuilderDescriptor
    {
        public Type BuilderType { get; }
        public ServiceDescriptor BuilderDescriptor { get; }
        public bool ReleaseAfterBuild { get; }

        public ConfigurePluginBuilderDescriptor(Type builderType, ServiceDescriptor builderDescriptor,
            bool releaseAfterBuild)
        {
            BuilderType = builderType;
            BuilderDescriptor = builderDescriptor;
            ReleaseAfterBuild = releaseAfterBuild;
        }
    }
}