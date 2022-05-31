﻿// <copyright file="InstallReceiver.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AdvancedSharpAdbClient.DeviceCommands
{
    /// <summary>
    /// Processes output of the <c>pm install</c> command.
    /// </summary>
    public class InstallReceiver : MultiLineReceiver
    {
        /// <summary>
        /// The error message that indicates an unkown error occurred.
        /// </summary>
        public const string UnknownError = "An unknown error occurred.";

        /// <summary>
        /// The message that indicates the operation completed successfully.
        /// </summary>
        private const string SuccessOutput = "Success";

        /// <summary>
        /// A regular expression that matches output of the success.
        /// </summary>
        private const string SuccessPattern = @"Success:\s+(.*)?";

        /// <summary>
        /// A regular expression that matches output that indicates a failure.
        /// </summary>
        private const string FailurePattern = @"Failure(?:\s+\[(.*)\])?";

        /// <summary>
        /// Gets the error message if the install was unsuccessful.
        /// </summary>
        public string? ErrorMessage { get; private set; }

        /// <summary>
        /// Gets the success message if the install is successful.
        /// </summary>
        public string? SuccessMessage { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the install was a success.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if success; otherwise, <see langword="false"/>.
        /// </value>
        public bool Success { get; private set; }

        /// <summary>
        /// Processes the new lines.
        /// </summary>
        /// <param name="lines">The lines.</param>
        protected override void ProcessNewLines(IEnumerable<string> lines)
        {
            foreach (string line in lines)
            {
                if (line.Length > 0)
                {
                    if (line.StartsWith(SuccessOutput))
                    {
                        Match? m = Regex.Match(line, SuccessPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                        SuccessMessage = SuccessOutput;

                        ErrorMessage = null;

                        if (m.Success)
                        {
                            string msg = m.Groups[1].Value;
                            SuccessMessage = msg.IsNullOrWhiteSpace() ? UnknownError : msg;
                        }

                        Success = true;
                    }
                    else
                    {
                        Match? m = Regex.Match(line, FailurePattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                        ErrorMessage = UnknownError;

                        SuccessMessage = null;

                        if (m.Success)
                        {
                            string msg = m.Groups[1].Value;
                            ErrorMessage = msg.IsNullOrWhiteSpace() ? UnknownError : msg;
                        }

                        Success = false;
                    }
                }
            }
        }
    }
}
