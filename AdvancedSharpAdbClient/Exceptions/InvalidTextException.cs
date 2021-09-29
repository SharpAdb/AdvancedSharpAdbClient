using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public InvalidTextException()
            : base("Text contains invalid symbols")
        {
        }
    }
}
