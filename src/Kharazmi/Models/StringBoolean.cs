namespace Kharazmi.Models
{
    public record StringBoolean : KeyValue<string, bool>
    {
        public StringBoolean()
        {
        }

        public StringBoolean(string key, bool value) : base(key, value)
        {
        }
    }
}