using System;

namespace Kharazmi.Options
{
    [AttributeUsage(AttributeTargets.Class)]
    public class InstanceWhenAttribute : Attribute
    {
        public Type OptionsType { get; }
        public string OptionKey { get;  }

        public InstanceWhenAttribute(Type optionsType, string optionKey)
        {
            OptionsType = optionsType;
            OptionKey = optionKey;
        }
    }
}