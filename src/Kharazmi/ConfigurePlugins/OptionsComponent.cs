using System;
using System.Collections.Generic;

namespace Kharazmi.ConfigurePlugins
{
    public enum ComponentType
    {
        TextBox,
        // TODO
    }
    public class OptionsComponent
    {
        public OptionsComponent(string optionsKey, string componentValue, TypeCode componentValueType, ComponentType componentType)
        {
            OptionsKey = optionsKey;
            ComponentValue = componentValue;
            ComponentValueType = componentValueType;
            ComponentType = componentType;
        }
        public string OptionsKey { get;  }
        public string ComponentValue { get;  }
        public TypeCode ComponentValueType { get;  }
        public ComponentType ComponentType { get;  }
    }

    public class OptionsComponentComposite : Dictionary<string, OptionsComponent>
    {
        public OptionsComponentComposite()
        {
            
        }
        
       // TODO

       public OptionsComponentComposite AddComponent(OptionsComponent value)
       {
           return this;
       }
    }
}