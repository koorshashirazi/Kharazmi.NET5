#region

using System;

#endregion

namespace Kharazmi.Pipelines
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class PipelineAttribute : Attribute
    {
        public PipelineAttribute(string name, PipelineType pipelineType)
        {
            Name = name;
            PipelineType = pipelineType;
        }

        public string Name { get; }
        public PipelineType PipelineType { get; }
    }
}