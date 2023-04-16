// <copyright file="ConsoleOutputReceiver.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using AdvancedSharpAdbClient.Exceptions;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace AdvancedSharpAdbClient.WinRT
{
    /// <summary>
    /// Recieves console output, and makes the console output available as a <see cref="string"/>. To
    /// fetch the console output that was received, used the <see cref="ToString"/> method.
    /// </summary>
    public sealed class ConsoleOutputReceiver : IMultiLineReceiver
    {
        private const RegexOptions DefaultRegexOptions = RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase;

        /// <summary>
        /// A <see cref="StringBuilder"/> which receives all output from the device.
        /// </summary>
        private readonly StringBuilder output = new();

        /// <summary>
        /// Gets or sets a value indicating whether [trim lines].
        /// </summary>
        /// <value><see langword="true"/> if [trim lines]; otherwise, <see langword="false"/>.</value>
        public bool TrimLines { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the receiver parses error messages.
        /// </summary>
        /// <value>
        ///     <see langword="true"/> if this receiver parsers error messages; otherwise <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// The default value is <see langword="false"/>. If set to <see langword="false"/>, the <see cref="AdbClient"/>
        /// will detect common error messages and throw an exception.
        /// </remarks>
        public bool ParsesErrors { get; set; }

        /// <summary>
        /// Gets or sets the lines.
        /// </summary>
        /// <value>The lines.</value>
        private ICollection<string> Lines { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleOutputReceiver"/> class.
        /// </summary>
        public ConsoleOutputReceiver() => Lines = new List<string>();

        /// <summary>
        /// Adds a line to the output.
        /// </summary>
        /// <param name="line">
        /// The line to add to the output.
        /// </param>
        public void AddOutput(string line) => Lines.Add(line);

        /// <summary>
        /// Gets a <see cref="string"/> that represents the current <see cref="ConsoleOutputReceiver"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> that represents the current <see cref="ConsoleOutputReceiver"/>.
        /// </returns>
        public override string ToString() => output.ToString();

        /// <summary>
        /// Throws an error message if the console output line contains an error message.
        /// </summary>
        /// <param name="line">
        /// The line to inspect.
        /// </param>
        public void ThrowOnError(string line)
        {
            if (!ParsesErrors)
            {
                if (line.EndsWith(": not found"))
                {
                    throw new FileNotFoundException($"The remote execution returned: '{line}'");
                }

                if (line.EndsWith("No such file or directory"))
                {
                    throw new FileNotFoundException($"The remote execution returned: '{line}'");
                }

                // for "unknown options"
                if (line.Contains("Unknown option"))
                {
                    throw new UnknownOptionException($"The remote execution returned: '{line}'");
                }

                // for "aborting" commands
                if (Regex.IsMatch(line, "Aborting.$", DefaultRegexOptions))
                {
                    throw new CommandAbortingException($"The remote execution returned: '{line}'");
                }

                // for busybox applets
                // cmd: applet not found
                if (Regex.IsMatch(line, "applet not found$", DefaultRegexOptions))
                {
                    throw new FileNotFoundException($"The remote execution returned: '{line}'");
                }

                // checks if the permission to execute the command was denied.
                // workitem: 16822
                if (Regex.IsMatch(line, "(permission|access) denied$", DefaultRegexOptions))
                {
                    throw new PermissionDeniedException($"The remote execution returned: '{line}'");
                }
            }
        }

        /// <summary>
        /// Flushes the output.
        /// </summary>
        public void Flush()
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
        private void Done()
        {
            // Do nothing
        }

        /// <summary>
        /// Processes the new lines.
        /// </summary>
        /// <param name="lines">The lines.</param>
        private void ProcessNewLines(IEnumerable<string> lines)
        {
            foreach (string line in lines)
            {
                if (string.IsNullOrEmpty(line) || line.StartsWith("#") || line.StartsWith("$"))
                {
                    continue;
                }

                output.AppendLine(line);
            }
        }
    }
}
