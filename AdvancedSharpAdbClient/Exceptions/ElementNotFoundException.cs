using System;

namespace AdvancedSharpAdbClient.Exceptions
{
    /// <summary>
    /// Represents an exception with Element
    /// </summary>
    public class ElementNotFoundException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ElementNotFoundException"/> class.
        /// </summary>
        public ElementNotFoundException(string message) : base(message)
        {
        }
    }
}
