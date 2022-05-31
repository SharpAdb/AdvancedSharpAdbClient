using System;

namespace AdvancedSharpAdbClient.Exceptions
{
    /// <summary>
    /// Represents an exception with keyevent
    /// </summary>
    public class InvalidKeyEventException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidKeyEventException"/> class.
        /// </summary>
        public InvalidKeyEventException(string message) : base(message)
        {
        }
    }
}
