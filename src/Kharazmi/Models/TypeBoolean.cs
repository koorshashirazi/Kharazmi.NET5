using System;

namespace Kharazmi.Models
{
    public record TypeBoolean : KeyValue<Type, bool>
    {
        public TypeBoolean()
        {
        }

        public TypeBoolean(Type key, bool value) : base(key, value)
        {
        }
    }
}