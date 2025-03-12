using System;
using Kharazmi.Common.ValueObjects;

namespace Kharazmi.RealTime
{
    public sealed class ConnectionId : Id<string>
    {
        public ConnectionId(string value) : base(value)
        {
        }

        public new static ConnectionId New => new(Guid.NewGuid().ToString("N"));
        public static ConnectionId From(string value) => new(value);
    }
}