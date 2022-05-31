using System;

namespace AdvancedSharpAdbClient.Exceptions
{
    /// <summary>
    /// Represents an exception with Element
    /// </summary>
    public class InvalidTextException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidTextException"/> class.
        /// </summary>
        public InvalidTextException() : base("Text contains invalid symbols")
        {
        }
    }
}
