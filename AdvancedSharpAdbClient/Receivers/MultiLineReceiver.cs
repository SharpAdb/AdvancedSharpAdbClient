// <copyright file="MultiLineReceiver.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Diagnostics;

namespace AdvancedSharpAdbClient.Receivers
{
    /// <summary>
    /// A multiline receiver to receive and process shell output with multi lines.
    /// </summary>
    [DebuggerDisplay($"{nameof(MultiLineReceiver)} \\{{ {nameof(TrimLines)} = {{{nameof(TrimLines)}}}, {nameof(ParsesErrors)} = {{{nameof(ParsesErrors)}}}, {nameof(Lines)} = {{{nameof(Lines)}}} }}")]
    public abstract partial class MultiLineReceiver : ShellOutputReceiver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MultiLineReceiver"/> class.
        /// </summary>
        public MultiLineReceiver() { }

        /// <summary>
        /// Gets or sets a value indicating whether [trim lines].
        /// </summary>
        /// <value><see langword="true"/> if [trim lines]; otherwise, <see langword="false"/>.</value>
        public bool TrimLines { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether the receiver parses error messages.
        /// </summary>
        /// <value><see langword="true"/> if this receiver parsers error messages; otherwise <see langword="false"/>.</value>
        /// <remarks>The default value is <see langword="true"/>. If set to <see langword="true"/>, the <see cref="AdbClient"/>
        /// will detect common error messages and throw an exception.</remarks>
        public virtual bool ParsesErrors { get; init; } = true;

        /// <summary>
        /// Gets or sets the lines.
        /// </summary>
        /// <value>The lines.</value>
        protected List<string> Lines { get; set; } = [];

        /// <inheritdoc/>
        public override bool AddOutput(string line)
        {
            ThrowOnError(line);
            Lines.Add(TrimLines ? line.Trim() : line);
            return true;
        }

        /// <inheritdoc/>
        public override void Flush()
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
        /// Throws an error message if the console output line contains an error message.
        /// </summary>
        /// <param name="line">The line to inspect.</param>
        protected virtual void ThrowOnError(string line)
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
