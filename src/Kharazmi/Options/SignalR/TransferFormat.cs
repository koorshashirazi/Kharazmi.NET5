using System.Collections.Generic;

namespace Kharazmi.Options.SignalR
{
    public static class TransferFormat
    {
        public static IReadOnlyCollection<string> Get() => new[] {Binary, Text};
        public const string Binary = "Binary";
        public const string Text = "Text";
    }
}