namespace Kharazmi.Runtime.Environments
{
    public static class EnvironmentHelper
    {
        public static string GetMachineName()
        {
            return System.Environment.GetEnvironmentVariable("CUMPUTERNAME") ??
                   System.Environment.GetEnvironmentVariable("HOSTNAME") ??
                   System.Environment.MachineName;
        }
    }
}