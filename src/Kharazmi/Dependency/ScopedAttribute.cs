using System;

namespace Kharazmi.Dependency
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class ScopedAttribute : Attribute
    {
    }
}