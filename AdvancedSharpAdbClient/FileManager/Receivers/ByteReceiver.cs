// <copyright file="ConsoleOutputReceiver.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;

namespace AdvancedSharpAdbClient
{
    /// <summary>
    /// Recieves console output, and makes the console output available as a <see cref="Array"/> of <see cref="byte"/>.
    /// </summary>
    public class ByteReceiver : MultiLineReceiver
    {
        /// <summary>
        /// A <see cref="Array"/> of <see cref="byte"/> which receives all output from the device.
        /// </summary>
        public byte[] Output = new byte[] { };

        /// <summary>
        /// Initializes a new instance of the <see cref="ByteReceiver"/> class.
        /// </summary>
        public ByteReceiver() { }

        /// <summary>
        /// Gets a <see cref="string"/> that represents the current <see cref="ByteReceiver"/>.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents the current <see cref="ByteReceiver"/>.</returns>
        public override string ToString()
        {
            return AdbClient.Encoding.GetString(Output);
        }

        /// <summary>
        /// Processes the new lines.
        /// </summary>
        /// <param name="lines">The lines.</param>
        protected override void ProcessNewLines(IEnumerable<string> lines)
        {
            foreach (string line in lines)
            {
                if (string.IsNullOrEmpty(line) || line.StartsWith("#") || line.StartsWith("$"))
                {
                    continue;
                }

                byte[] bytes = AdbClient.Encoding.GetBytes($"\n{line}");
                byte[] results = new byte[Output.Length + bytes.Length];

                Output.CopyTo(results, 0);
                bytes.CopyTo(results, Output.Length);

                Output = results;
            }
        }
    }
}
