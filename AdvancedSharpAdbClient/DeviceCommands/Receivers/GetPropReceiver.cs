// <copyright file="GetPropReceiver.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace AdvancedSharpAdbClient.DeviceCommands.Receivers
{
    /// <summary>
    /// Parses the output of the <c>getprop</c> command, which lists all properties of an Android device.
    /// </summary>
    [DebuggerDisplay($"{nameof(GetPropReceiver)} \\{{ {nameof(Properties)} = {{{nameof(Properties)}}} }}")]
    public sealed partial class GetPropReceiver : ShellOutputReceiver
    {
        /// <summary>
        /// The path to the <c>getprop</c> executable to run on the device.
        /// </summary>
        public const string GetPropCommand = "/system/bin/getprop";

        /// <summary>
        /// A regular expression which can be used to parse the <c>getprop</c> output.
        /// </summary>
        private const string GetPropPattern = @"^\[([^]]+)\]\:\s*\[(.*)\]$";

        /// <summary>
        /// The <see cref="Regex"/> cached by <see cref="AddOutput(string)"/>.
        /// </summary>
        private Regex? regex = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetPropReceiver"/> class.
        /// </summary>
        public GetPropReceiver() { }

        /// <summary>
        /// Gets the list of properties which have been retrieved.
        /// </summary>
        public Dictionary<string, string> Properties { get; } = [];

        /// <inheritdoc/>
        [MemberNotNull(nameof(regex))]
        public override bool AddOutput(string line)
        {
            regex ??= GetPropRegex();

            // We receive an array of lines. We're expecting
            // to have the build info in the first line, and the build
            // date in the 2nd line. There seems to be an empty line
            // after all that.
            if (string.IsNullOrEmpty(line) || line.StartsWith('#') || line.StartsWith('$'))
            {
                return true;
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
            return true;
        }

        /// <inheritdoc/>
        protected override void Done() => regex = null;

#if NET7_0_OR_GREATER
        [GeneratedRegex(GetPropPattern)]
        private static partial Regex GetPropRegex();
#else
        /// <summary>
        /// Gets a <see cref="Regex"/> which can be used to parse the <c>getprop</c> output.
        /// </summary>
        /// <returns>The <see cref="Regex"/> which can be used to parse the <c>getprop</c> output.</returns>
        private static Regex GetPropRegex() => new(GetPropPattern);
#endif
    }
}
