using Kharazmi.Options.Domain;

namespace Kharazmi.Options
{
    public interface IChannelOptions
    {
        string? ChannelPrefix { get; set; }
        RetryOption RetryOption { get; set; }
        bool PublishAsynchronous { get; set; }
        bool SubscribeAsynchronous { get; set; }
        bool ThrowExceptionOnSubscribeFailed { get; set; }

        string? ConfigurationChannel { get; set; }

        /// <summary> </summary>
        string DefaultPatternMode { get; set; }

        /// <summary> </summary>
        string DefaultCommandFlags { get; set; }
    }
}