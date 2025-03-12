namespace Kharazmi.Common.Channels
{
    public enum PatternMode
    {
        /// <summary>
        /// Will be treated as a pattern if it includes *
        /// </summary>
        Auto = 0,
        /// <summary>
        /// Never a pattern
        /// </summary>
        Literal = 1,
        /// <summary>
        /// Always a pattern
        /// </summary>
        Pattern = 2
    }
}