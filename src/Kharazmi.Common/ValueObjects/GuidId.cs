#region

using System;

#endregion

namespace Kharazmi.Common.ValueObjects
{
    /// <summary></summary>
    public class GuidId : Identity<GuidId, Guid>
    {
        /// <summary></summary>
        public GuidId(Guid value = default)
            : base(value == default ? Guid.NewGuid() : value)
        {
        }
    }
}