#region

using System;

#endregion

namespace Kharazmi.Localization
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class TableAttribute : Attribute
    {
        public string Name { get; }

        public TableAttribute(string name)
        {
            Name = name;
        }
    }
}