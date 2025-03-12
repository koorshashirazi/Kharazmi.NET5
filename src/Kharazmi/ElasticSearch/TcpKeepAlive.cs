using System;

namespace Kharazmi.ElasticSearch
{
    public class TcpKeepAlive
    {
        public TcpKeepAlive()
        {
        }

        public TcpKeepAlive(TimeSpan keepAliveTime, TimeSpan keepAliveInterval)
        {
            KeepAliveTime = keepAliveTime;
            KeepAliveInterval = keepAliveInterval;
        }

        public TimeSpan KeepAliveTime { get; set; }
        public TimeSpan KeepAliveInterval { get; set; }
    }
}