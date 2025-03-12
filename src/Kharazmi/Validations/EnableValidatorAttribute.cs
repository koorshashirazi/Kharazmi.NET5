#region

using System;

#endregion

namespace Kharazmi.Validations
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class EnableValidatorAttribute : Attribute
    {
        public bool Enable { get; set; }
    }
}