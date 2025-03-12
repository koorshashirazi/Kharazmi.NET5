using System;

namespace Kharazmi.Common.ValueObjects
{
    public class UserId : Id<string>
    {
        public UserId(string value) : base(value)
        {
        }

        public new static UserId New => new UserId(Guid.NewGuid().ToString("N"));
    }
}