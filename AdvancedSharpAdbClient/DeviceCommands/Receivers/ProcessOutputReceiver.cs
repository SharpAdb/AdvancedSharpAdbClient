// <copyright file="ProcessOutputReceiver.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System.Collections.Generic;

namespace AdvancedSharpAdbClient.Receivers.DeviceCommands
{
    /// <summary>
    /// Parses the output of a <c>cat /proc/[pid]/stat</c> command.
    /// </summary>
    public class ProcessOutputReceiver : MultiLineReceiver
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
        protected override void ProcessNewLines(IEnumerable<string> lines)
        {
            foreach (string line in lines)
            {
                // Process has already died (e.g. the cat process itself)
                if (line.Contains("No such file or directory"))
                {
                    continue;
                }

                try
                {
                    Processes.Add(new AndroidProcess(line, cmdLinePrefix: true));
                }
                catch
                {
                    // Swallow
                }
            }
        }
    }
}
