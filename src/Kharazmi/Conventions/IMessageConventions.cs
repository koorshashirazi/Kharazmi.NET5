namespace Kharazmi.Conventions
{
    /// <summary>_</summary>
    public interface IMessageConventions
    {
        /// <summary>_</summary>
        string RoutingKey { get; }

        /// <summary>_</summary>
        string Exchange { get; }

        /// <summary>_</summary>
        string Queue { get; }
    }
}