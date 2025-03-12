#region

using System;

#endregion

namespace Kharazmi.Validations
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class SkipValidatorAttribute : Attribute
    {
    }
}