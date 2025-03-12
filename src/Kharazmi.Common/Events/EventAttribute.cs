#region

using System;

#endregion

namespace Kharazmi.Common.Events
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class EventAttribute : Attribute
    {
        public string Name { get; }

        public EventAttribute(string name)
        {
            Name = name;
        }
    }
}