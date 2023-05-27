// <copyright file="CrossPlatformFunc.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace AdvancedSharpAdbClient
{
    /// <summary>
    /// The functions which are used by the <see cref="IAdbCommandLineClient"/> class, but which are platform-specific.
    /// </summary>
    public static class CrossPlatformFunc
    {
        /// <summary>
        /// Determines whether the specified file exists.
        /// </summary>
        public static Func<string, bool> CheckFileExists { get; set; } = File.Exists;

        /// <summary>
        /// Runs process, invoking a specific command, and reads the standard output and standard error output.
        /// </summary>
        /// <returns>The return code of the process.</returns>
        public static Func<string, string, List<string>, List<string>, int> RunProcess { get; set; } = (string filename, string command, List<string> errorOutput, List<string> standardOutput) =>
        {
#if HAS_PROCESS
            ProcessStartInfo psi = new(filename, command)
            {
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };

            using Process process = Process.Start(psi);
            string standardErrorString = process.StandardError.ReadToEnd();
            string standardOutputString = process.StandardOutput.ReadToEnd();

            errorOutput?.AddRange(standardErrorString.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));

            standardOutput?.AddRange(standardOutputString.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));

            // get the return code from the process
            if (!process.WaitForExit(5000))
            {
                process.Kill();
            }

            return process.ExitCode;
#else
            throw new PlatformNotSupportedException();
#endif
        };
    }
}
