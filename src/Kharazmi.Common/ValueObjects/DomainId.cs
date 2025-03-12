using System;

namespace Kharazmi.Common.ValueObjects
{
    public class DomainId : Id<string>
    {
        public DomainId(string value) : base(value)
        {
        }

        public new static DomainId New => new DomainId(Guid.NewGuid().ToString("N"));
        public new static DomainId FromString(string value) => new DomainId(value);
    }
}