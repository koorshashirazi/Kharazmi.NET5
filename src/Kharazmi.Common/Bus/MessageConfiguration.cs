#region

#endregion

namespace Kharazmi.Common.Bus
{
    /// <summary></summary>
    public class MessageConfiguration
    {
        /// <summary>_</summary>
        public MessageConfiguration()
        {
            ExchangeConfiguration = new ExchangeConfiguration();
            QueueConfiguration = new QueueConfiguration();
        }
        /// <summary></summary>
        public ExchangeConfiguration ExchangeConfiguration { get; set; }

        /// <summary></summary>
        public QueueConfiguration QueueConfiguration { get; set; }
    }
}