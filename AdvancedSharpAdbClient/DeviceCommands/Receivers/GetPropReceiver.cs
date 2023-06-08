// <copyright file="GetPropReceiver.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AdvancedSharpAdbClient.DeviceCommands
{
    /// <summary>
    /// Parses the output of the <c>getprop</c> command, which lists all properties of an Android device.
    /// </summary>
    public sealed partial class GetPropReceiver : MultiLineReceiver
    {
        /// <summary>
        /// The path to the <c>getprop</c> executable to run on the device.
        /// </summary>
        public const string GetPropCommand = "/system/bin/getprop";

        /// <summary>
        /// A regular expression which can be used to parse the <c>getprop</c> output.
        /// </summary>
        private const string GetPropPattern = "^\\[([^]]+)\\]\\:\\s*\\[(.*)\\]$";

        /// <summary>
        /// Gets the list of properties which have been retrieved.
        /// </summary>
        public Dictionary<string, string> Properties { get; private set; } = new Dictionary<string, string>();

        /// <summary>
        /// Processes the new lines.
        /// </summary>
        /// <param name="lines">The lines to process.</param>
        protected override void ProcessNewLines(IEnumerable<string> lines)
        {
            Regex regex = GetPropRegex();

            // We receive an array of lines. We're expecting
            // to have the build info in the first line, and the build
            // date in the 2nd line. There seems to be an empty line
            // after all that.
            foreach (string line in lines)
            {
                if (string.IsNullOrEmpty(line) || line.StartsWith("#") || line.StartsWith("$"))
                {
                    continue;
                }
                Match m = regex.Match(line);
                if (m.Success)
                {
                    string label = m.Groups[1].Value.Trim();
                    string value = m.Groups[2].Value.Trim();

                    if (label.Length > 0)
                    {
                        Properties[label] = value;
                    }
                }
            }
        }

#if NET7_0_OR_GREATER
        [GeneratedRegex(GetPropPattern)]
        private static partial Regex GetPropRegex();
#else
        private static Regex GetPropRegex() => new(GetPropPattern);
#endif
    }
}
