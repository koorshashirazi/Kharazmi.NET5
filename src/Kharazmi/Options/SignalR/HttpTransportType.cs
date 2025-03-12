using System.Collections.Generic;

namespace Kharazmi.Options.SignalR
{
    public static class HttpTransportType
    {
        public static IReadOnlyCollection<string> Get()
            => new[] {None, Default, WebSockets, ServerSendEvents, LongPolling};

        public const string None = "None";
        public const string Default = "Default";
        public const string WebSockets = "WebSockets";
        public const string ServerSendEvents = "ServerSendEvents";
        public const string LongPolling = "longPolling";
    }
}