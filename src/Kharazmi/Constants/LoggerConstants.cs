namespace Kharazmi.Constants
{
    public static class LoggerConstants
    {
        public const int DefaultRetainedFileCountLimit = 31;
        public const long DefaultFileSizeLimitBytes = 1L * 1024 * 1024 * 1024;
        public const string DefaultOutputTemplate =
            "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}";
    }
}