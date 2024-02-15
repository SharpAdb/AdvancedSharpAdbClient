// <copyright file="ProcessOutputReceiver.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Diagnostics;

namespace AdvancedSharpAdbClient.DeviceCommands.Receivers
{
    /// <summary>
    /// Parses the output of a <c>cat /proc/[pid]/stat</c> command.
    /// </summary>
    [DebuggerDisplay($"{nameof(ProcessOutputReceiver)} \\{{ {nameof(Processes)} = {{{nameof(Processes)}}} }}")]
    public class ProcessOutputReceiver : ShellOutputReceiver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessOutputReceiver"/> class.
        /// </summary>
        public ProcessOutputReceiver() { }

        /// <summary>
        /// Gets a list of all processes that have been received.
        /// </summary>
        public List<AndroidProcess> Processes { get; } = [];

        /// <inheritdoc/>
        public override bool AddOutput(string line)
        {
            // Process has already died (e.g. the cat process itself)
            if (line.Contains("No such file or directory"))
            {
                return false;
            }

            try
            {
                Processes.Add(new AndroidProcess(line, cmdLinePrefix: true));
            }
            catch
            {
                // Swallow
            }
            return true;
        }
    }
}
