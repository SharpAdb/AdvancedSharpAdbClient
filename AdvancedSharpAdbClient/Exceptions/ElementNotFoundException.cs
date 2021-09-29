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
    public class ElementNotFoundException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ElementNotFoundException"/> class.
        /// </summary>
        public ElementNotFoundException(string message) 
            : base(message)
        {
        }
    }
}
