// <copyright file="CrossPlatformFunc.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace AdvancedSharpAdbClient
{
    /// <summary>
    /// The functions which are used by the <see cref="IAdbCommandLineClient"/> class, but which are platform-specific.
    /// </summary>
    public static class CrossPlatformFunc
    {
        private static readonly char[] separator = ['\r', '\n'];

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

            errorOutput?.AddRange(standardErrorString.Split(separator, StringSplitOptions.RemoveEmptyEntries));

            standardOutput?.AddRange(standardOutputString.Split(separator, StringSplitOptions.RemoveEmptyEntries));

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

#if HAS_TASK
#if NETFRAMEWORK && !NET40_OR_GREATER
        /// <summary>
        /// Encapsulates a method that has five parameters and returns a value of the type specified by the <typeparamref name="TResult"/> parameter.
        /// </summary>
        /// <returns>The return value of the method that this delegate encapsulates.</returns>
        public delegate TResult Func<in T1, in T2, in T3, in T4, in T5, out TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
#endif

        /// <summary>
        /// Runs process, invoking a specific command, and reads the standard output and standard error output.
        /// </summary>
        /// <returns>The return code of the process.</returns>
        public static Func<string, string, List<string>, List<string>, CancellationToken, Task<int>> RunProcessAsync { get; set; } =
#if HAS_PROCESS
            async
#endif
            (string filename, string command, List<string> errorOutput, List<string> standardOutput, CancellationToken cancellationToken) =>
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
            string standardErrorString = await process.StandardError.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
            string standardOutputString = await process.StandardOutput.ReadToEndAsync(cancellationToken).ConfigureAwait(false);

            errorOutput?.AddRange(standardErrorString.Split(separator, StringSplitOptions.RemoveEmptyEntries));

            standardOutput?.AddRange(standardOutputString.Split(separator, StringSplitOptions.RemoveEmptyEntries));

#if NET5_0_OR_GREATER
            using (CancellationTokenSource completionSource = new(TimeSpan.FromMilliseconds(5000)))
            {
                await process.WaitForExitAsync(completionSource.Token);
                if (!process.HasExited)
                {
                    process.Kill();
                }
            }
#else
            // get the return code from the process
            if (!process.WaitForExit(5000))
            {
                process.Kill();
            }
#endif
            return process.ExitCode;
#else
            TaskCompletionSource<int> source = new();
            source.SetException(new PlatformNotSupportedException());
            return source.Task;
#endif
        };
#endif
    }
}
