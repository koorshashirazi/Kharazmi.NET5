#region

using System;

#endregion

namespace Kharazmi.Common.ValueObjects
{
    /// <summary> </summary>
    public class StringId : Identity<StringId,string>
    {
        /// <summary> </summary>
        public StringId(string? value = "")
            : base(string.IsNullOrWhiteSpace(value) ? Guid.NewGuid().ToString("N") : value)
        {
        }
    }
}