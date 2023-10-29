// <copyright file="MultiLineReceiver.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System.Collections.Generic;

namespace AdvancedSharpAdbClient.Receivers
{
    /// <summary>
    /// A multiline receiver to receive and process shell output with multi lines.
    /// </summary>
    public abstract class MultiLineReceiver : IShellOutputReceiver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MultiLineReceiver"/> class.
        /// </summary>
        public MultiLineReceiver() => Lines = new List<string>();

        /// <summary>
        /// Gets or sets a value indicating whether [trim lines].
        /// </summary>
        /// <value><see langword="true"/> if [trim lines]; otherwise, <see langword="false"/>.</value>
        public bool TrimLines { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the receiver parses error messages.
        /// </summary>
        /// <value><see langword="true"/> if this receiver parsers error messages; otherwise <see langword="false"/>.</value>
        /// <remarks>The default value is <see langword="false"/>. If set to <see langword="false"/>, the <see cref="AdbClient"/>
        /// will detect common error messages and throw an exception.</remarks>
        public virtual bool ParsesErrors { get; protected set; }

        /// <summary>
        /// Gets or sets the lines.
        /// </summary>
        /// <value>The lines.</value>
        protected ICollection<string> Lines { get; set; }

        /// <summary>
        /// Adds a line to the output.
        /// </summary>
        /// <param name="line">The line to add to the output.</param>
        public virtual void AddOutput(string line) => Lines.Add(line);

        /// <summary>
        /// Flushes the output.
        /// </summary>
        public virtual void Flush()
        {
            if (Lines.Count > 0)
            {
                // send it for final processing
                ProcessNewLines(Lines);
                Lines.Clear();
            }

            Done();
        }

        /// <summary>
        /// Finishes the receiver.
        /// </summary>
        protected virtual void Done()
        {
            // Do nothing
        }

        /// <summary>
        /// Processes the new lines.
        /// </summary>
        /// <param name="lines">The lines.</param>
        protected abstract void ProcessNewLines(IEnumerable<string> lines);
    }
}
