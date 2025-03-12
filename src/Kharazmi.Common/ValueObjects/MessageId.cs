using System;

namespace Kharazmi.Common.ValueObjects
{
    public class MessageId : Id<string>
    {
        public MessageId(string value) : base(value)
        {
        }

        public new static MessageId New => new MessageId(Guid.NewGuid().ToString("N"));
        public new static MessageId FromString(string value) => new MessageId(value);
        
        public static MessageId FromType(Type type) => new MessageId($"{type.Name}-{Guid.NewGuid():N}");
    }
}