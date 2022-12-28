using System.Collections.Generic;

namespace AdvancedSharpAdbClient
{
    /// <summary>
    /// Lines Collection Receiver
    /// </summary>
    public class LinesCollectionReceiver : IShellOutputReceiver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LinesCollectionReceiver"/> class.
        /// </summary>
        public LinesCollectionReceiver()
        {
            this.Lines = new List<string>();
        }

        /// <summary>
        /// Gets or sets a value indicating whether the receiver parses error messages.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this receiver parsers error messages; otherwise <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// The default value is <see langword="false"/>. If set to <see langword="false"/>, the <see cref="AdbClient"/>
        /// will detect common error messages and throw an exception.
        /// </remarks>
        public virtual bool ParsesErrors { get; protected set; }

        /// <summary>
        /// Gets or sets the lines.
        /// </summary>
        /// <value>The lines.</value>
        public ICollection<string> Lines { get; set; }

        /// <summary>
        /// Adds a line to the output.
        /// </summary>
        /// <param name="line">
        /// The line to add to the output.
        /// </param>
        public void AddOutput(string line) => Lines.Add(line);

        /// <summary>
        /// Flushes the output.
        /// </summary>
        public void Flush()
        {
            // Do nothing
        }
    }
}
