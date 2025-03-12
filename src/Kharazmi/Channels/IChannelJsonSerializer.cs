namespace Kharazmi.Channels
{
    public interface IChannelJsonSerializer
    {
        byte[] Serialize(object item);
        byte[]? SerializeEvent(object? item);
        T Deserialize<T>(byte[] serializedBytes);
    }
}