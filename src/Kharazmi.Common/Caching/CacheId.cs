using System;
using Kharazmi.Common.ValueObjects;

namespace Kharazmi.Common.Caching
{
    public class CacheId: Id<string>
    {
        public CacheId(string value) : base(value)
        {
        }

        public new static CacheId New => new CacheId(Guid.NewGuid().ToString("N"));
        public static CacheId From(string value) => new CacheId(value);
    }
}