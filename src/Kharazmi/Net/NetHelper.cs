using System;

namespace Kharazmi.Net
{
    public static class NetHelper
    {
        public static bool CanUsingCurlHandler()
        {
            var curlHandlerExists =
                typeof(System.Net.Http.HttpClientHandler).Assembly.GetType("System.Net.Http.CurlHandler") != null;
            if (!curlHandlerExists) return false;

            var socketsHandlerExists =
                typeof(System.Net.Http.HttpClientHandler).Assembly.GetType("System.Net.Http.SocketsHttpHandler") !=
                null;
            if (!socketsHandlerExists) return true;

            if (AppContext.TryGetSwitch("System.Net.Http.UseSocketsHttpHandler", out var isEnabled))
                return !isEnabled;

            var environmentVariable =
                Environment.GetEnvironmentVariable("DOTNET_SYSTEM_NET_HTTP_USESOCKETSHTTPHANDLER");

            if (environmentVariable == null) return false;

            return environmentVariable.Equals("false", StringComparison.OrdinalIgnoreCase) ||
                   environmentVariable.Equals("0");
        }
    }
}