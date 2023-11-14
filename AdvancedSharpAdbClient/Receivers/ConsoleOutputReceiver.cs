// <copyright file="ConsoleOutputReceiver.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace AdvancedSharpAdbClient.Receivers
{
    /// <summary>
    /// Receives console output, and makes the console output available as a <see cref="string"/>. To
    /// fetch the console output that was received, used the <see cref="ToString"/> method.
    /// </summary>
    /// <param name="logger">The logger to use when logging.</param>
    public partial class ConsoleOutputReceiver(ILogger<ConsoleOutputReceiver>? logger = null) : MultiLineReceiver
    {
        /// <summary>
        /// The default <see cref="RegexOptions"/> to use when parsing the output.
        /// </summary>
        protected const RegexOptions DefaultRegexOptions = RegexOptions.Singleline | RegexOptions.IgnoreCase;

        /// <summary>
        /// The logger to use when logging messages.
        /// </summary>
        protected readonly ILogger<ConsoleOutputReceiver> logger = logger ?? LoggerProvider.CreateLogger<ConsoleOutputReceiver>();

        /// <summary>
        /// A <see cref="StringBuilder"/> which receives all output from the device.
        /// </summary>
        protected readonly StringBuilder output = new();

        /// <summary>
        /// Gets a <see cref="string"/> that represents the current <see cref="ConsoleOutputReceiver"/>.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents the current <see cref="ConsoleOutputReceiver"/>.</returns>
        public override string ToString() => output.ToString();

        /// <summary>
        /// Throws an error message if the console output line contains an error message.
        /// </summary>
        /// <param name="line">The line to inspect.</param>
        public virtual void ThrowOnError(string line)
        {
            if (!ParsesErrors)
            {
                if (line.EndsWith(": not found"))
                {
                    logger.LogWarning($"The remote execution returned: '{line}'");
                    throw new FileNotFoundException($"The remote execution returned: '{line}'");
                }

                if (line.EndsWith("No such file or directory"))
                {
                    logger.LogWarning($"The remote execution returned: {line}");
                    throw new FileNotFoundException($"The remote execution returned: '{line}'");
                }

                // for "unknown options"
                if (line.Contains("Unknown option"))
                {
                    logger.LogWarning($"The remote execution returned: {line}");
                    throw new UnknownOptionException($"The remote execution returned: '{line}'");
                }

                // for "aborting" commands
                if (AbortingRegex().IsMatch(line))
                {
                    logger.LogWarning($"The remote execution returned: {line}");
                    throw new CommandAbortingException($"The remote execution returned: '{line}'");
                }

                // for busybox applets
                // cmd: applet not found
                if (AppletRegex().IsMatch(line))
                {
                    logger.LogWarning($"The remote execution returned: '{line}'");
                    throw new FileNotFoundException($"The remote execution returned: '{line}'");
                }

                // checks if the permission to execute the command was denied.
                // workitem: 16822
                if (DeniedRegex().IsMatch(line))
                {
                    logger.LogWarning($"The remote execution returned: '{line}'");
                    throw new PermissionDeniedException($"The remote execution returned: '{line}'");
                }
            }
        }

        /// <summary>
        /// Processes the new lines.
        /// </summary>
        /// <param name="lines">The lines.</param>
        protected override void ProcessNewLines(IEnumerable<string> lines)
        {
            foreach (string line in lines)
            {
                if (string.IsNullOrEmpty(line) || line.StartsWith('#') || line.StartsWith('$'))
                {
                    continue;
                }
                output.AppendLine(line);
                logger.LogDebug(line);
            }
        }

#if NET7_0_OR_GREATER
        [GeneratedRegex("Aborting.$", DefaultRegexOptions)]
        private static partial Regex AbortingRegex();

        [GeneratedRegex("applet not found$", DefaultRegexOptions)]
        private static partial Regex AppletRegex();

        [GeneratedRegex("(permission|access) denied$", DefaultRegexOptions)]
        private static partial Regex DeniedRegex();
#else
        private static Regex AbortingRegex() => new("Aborting.$", DefaultRegexOptions);

        private static Regex AppletRegex() => new("applet not found$", DefaultRegexOptions);

        private static Regex DeniedRegex() => new("(permission|access) denied$", DefaultRegexOptions);
#endif
    }
}
