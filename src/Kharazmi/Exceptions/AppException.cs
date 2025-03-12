namespace Kharazmi.Exceptions
{
    public abstract class AppException : FrameworkException
    {
        protected AppException(string message) : base(message)
        {
        }
    }
}