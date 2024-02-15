// <copyright file="EnvironmentVariablesReceiver.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace AdvancedSharpAdbClient.DeviceCommands.Receivers
{
    /// <summary>
    /// Processes the output of the <c>printenv</c> command, which dumps all environment variables of an Android device.
    /// </summary>
    [DebuggerDisplay($"{nameof(EnvironmentVariablesReceiver)} \\{{ {nameof(EnvironmentVariables)} = {{{nameof(EnvironmentVariables)}}} }}")]
    public sealed partial class EnvironmentVariablesReceiver : ShellOutputReceiver
    {
        /// <summary>
        /// The path to the <c>printenv</c> command.
        /// </summary>
        public const string PrintEnvCommand = "/system/bin/printenv";

        /// <summary>
        /// A regular expression that can be used to parse the output of the <c>printenv</c> command.
        /// </summary>
        private const string EnvPattern = @"^([^=\s]+)\s*=\s*(.*)$";

        /// <summary>
        /// The <see cref="Regex"/> cached by <see cref="AddOutput(string)"/>.
        /// </summary>
        private Regex? regex = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnvironmentVariablesReceiver"/> class.
        /// </summary>
        public EnvironmentVariablesReceiver() { }

        /// <summary>
        /// Gets the environment variables that are currently defined on the device.
        /// </summary>
        public Dictionary<string, string> EnvironmentVariables { get; } = [];

        /// <inheritdoc/>
        [MemberNotNull(nameof(regex))]
        public override bool AddOutput(string line)
        {
            regex ??= EnvRegex();
            if (string.IsNullOrEmpty(line) || line.StartsWith('#'))
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
                    EnvironmentVariables[label] = value;
                }
            }
            return true;
        }

        /// <inheritdoc/>
        protected override void Done() => regex = null;

#if NET7_0_OR_GREATER
        [GeneratedRegex(EnvPattern)]
        private static partial Regex EnvRegex();
#else
        /// <summary>
        /// Gets a <see cref="Regex"/> which can be used to parse the output of the <c>printenv</c> command.
        /// </summary>
        /// <returns>The <see cref="Regex"/> which can be used to parse the output of the <c>printenv</c> command.</returns>
        private static Regex EnvRegex() => new(EnvPattern);
#endif
    }
}
