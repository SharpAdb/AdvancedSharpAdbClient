﻿// <copyright file="ConsoleOutputReceiver.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

using AdvancedSharpAdbClient.Exceptions;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

#if !NET35 && !NET40
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
#endif

namespace AdvancedSharpAdbClient
{
    /// <summary>
    /// Recieves console output, and makes the console output available as a <see cref="string"/>. To
    /// fetch the console output that was received, used the <see cref="ToString"/> method.
    /// </summary>
    public class ConsoleOutputReceiver : MultiLineReceiver
    {
        private const RegexOptions DefaultRegexOptions = RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase;

#if !NET35 && !NET40
        /// <summary>
        /// The logger to use when logging messages.
        /// </summary>
        private readonly ILogger<ConsoleOutputReceiver> logger;
#endif

        /// <summary>
        /// A <see cref="StringBuilder"/> which receives all output from the device.
        /// </summary>
        private readonly StringBuilder output = new StringBuilder();

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleOutputReceiver"/> class.
        /// </summary>
        /// <param name="logger">
        /// The logger to use when logging.
        /// </param>
        public ConsoleOutputReceiver(
#if !NET35 && !NET40
            ILogger<ConsoleOutputReceiver> logger = null
#endif
            )
        {
#if !NET35 && !NET40
            this.logger = logger ?? NullLogger<ConsoleOutputReceiver>.Instance;
#endif
        }

        /// <summary>
        /// Gets a <see cref="string"/> that represents the current <see cref="ConsoleOutputReceiver"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> that represents the current <see cref="ConsoleOutputReceiver"/>.
        /// </returns>
        public override string ToString()
        {
            return output.ToString();
        }

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
#if !NET35 && !NET40
                    logger.LogWarning($"The remote execution returned: '{line}'");
#endif
                    throw new FileNotFoundException($"The remote execution returned: '{line}'");
                }

                if (line.EndsWith("No such file or directory"))
                {
#if !NET35 && !NET40
                    logger.LogWarning($"The remote execution returned: {line}");
#endif
                    throw new FileNotFoundException($"The remote execution returned: '{line}'");
                }

                // for "unknown options"
                if (line.Contains("Unknown option"))
                {
#if !NET35 && !NET40
                    logger.LogWarning($"The remote execution returned: {line}");
#endif
                    throw new UnknownOptionException($"The remote execution returned: '{line}'");
                }

                // for "aborting" commands
                if (Regex.IsMatch(line, "Aborting.$", DefaultRegexOptions))
                {
#if !NET35 && !NET40
                    logger.LogWarning($"The remote execution returned: {line}");
#endif
                    throw new CommandAbortingException($"The remote execution returned: '{line}'");
                }

                // for busybox applets
                // cmd: applet not found
                if (Regex.IsMatch(line, "applet not found$", DefaultRegexOptions))
                {
#if !NET35 && !NET40
                    logger.LogWarning($"The remote execution returned: '{line}'");
#endif
                    throw new FileNotFoundException($"The remote execution returned: '{line}'");
                }

                // checks if the permission to execute the command was denied.
                // workitem: 16822
                if (Regex.IsMatch(line, "(permission|access) denied$", DefaultRegexOptions))
                {
#if !NET35 && !NET40
                    logger.LogWarning($"The remote execution returned: '{line}'");
#endif
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
            foreach (string? line in lines)
            {
                if (string.IsNullOrEmpty(line) || line.StartsWith("#") || line.StartsWith("$"))
                {
                    continue;
                }

                output.AppendLine(line);

#if !NET35 && !NET40
                logger.LogDebug(line);
#endif
            }
        }
    }
}
