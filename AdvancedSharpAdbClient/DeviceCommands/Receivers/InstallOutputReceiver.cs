// <copyright file="InstallOutputReceiver.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AdvancedSharpAdbClient.DeviceCommands
{
    /// <summary>
    /// Processes output of the <c>pm install</c> command.
    /// </summary>
    public partial class InstallOutputReceiver : MultiLineReceiver
    {
        /// <summary>
        /// The error message that indicates an unknown error occurred.
        /// </summary>
        public const string UnknownError = "An unknown error occurred.";

        /// <summary>
        /// The message that indicates the operation completed successfully.
        /// </summary>
        private const string SuccessOutput = "Success";

        /// <summary>
        /// The message that indicates the operation indicates a failure.
        /// </summary>
        private const string FailureOutput = "Failure";

        /// <summary>
        /// A regular expression that matches output of the success.
        /// </summary>
        private const string SuccessPattern = @"Success:\s+(.*)?";

        /// <summary>
        /// A regular expression that matches output that indicates a failure.
        /// </summary>
        private const string FailurePattern = @"Failure(?:\s+\[(.*)\])?";

        /// <summary>
        /// A regular expression that matches output that indicates a error.
        /// </summary>
        private const string ErrorPattern = @"Error:\s+(.*)?";

        /// <summary>
        /// Gets the error message if the install was unsuccessful.
        /// </summary>
        public string ErrorMessage { get; private set; }

        /// <summary>
        /// Gets the success message if the install is successful.
        /// </summary>
        public string SuccessMessage { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the install was a success.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if success; otherwise, <see langword="false"/>.
        /// </value>
        public bool Success { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InstallOutputReceiver"/> class.
        /// </summary>
        public InstallOutputReceiver() { }

        /// <summary>
        /// Processes the new lines.
        /// </summary>
        /// <param name="lines">The lines.</param>
        protected override void ProcessNewLines(IEnumerable<string> lines)
        {
            Regex successRegex = SuccessRegex();
            Regex failureRegex = FailureRegex();
            Regex errorRegex = ErrorRegex();

            foreach (string line in lines)
            {
                if (line.Length > 0)
                {
                    if (line.StartsWith(SuccessOutput))
                    {
                        Match m = successRegex.Match(line);
                        SuccessMessage = string.Empty;

                        ErrorMessage = null;

                        if (m.Success)
                        {
                            string msg = m.Groups[1].Value;
                            SuccessMessage = msg ?? string.Empty;
                        }

                        Success = true;
                    }
                    else if (line.StartsWith(FailureOutput))
                    {
                        Match m = failureRegex.Match(line);
                        ErrorMessage = UnknownError;

                        SuccessMessage = null;

                        if (m.Success)
                        {
                            string msg = m.Groups[1].Value;
                            ErrorMessage = msg.IsNullOrWhiteSpace() ? UnknownError : msg;
                        }

                        Success = false;
                    }
                    else
                    {
                        Match m = errorRegex.Match(line);
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

#if NET7_0_OR_GREATER
        [GeneratedRegex(SuccessPattern, RegexOptions.IgnoreCase)]
        private static partial Regex SuccessRegex();

        [GeneratedRegex(FailurePattern, RegexOptions.IgnoreCase)]
        private static partial Regex FailureRegex();

        [GeneratedRegex(ErrorPattern, RegexOptions.IgnoreCase)]
        private static partial Regex ErrorRegex();
#else
        private static Regex SuccessRegex() => new(SuccessPattern, RegexOptions.IgnoreCase);

        private static Regex FailureRegex() => new(FailurePattern, RegexOptions.IgnoreCase);

        private static Regex ErrorRegex() => new(ErrorPattern, RegexOptions.IgnoreCase);
#endif
    }
}
