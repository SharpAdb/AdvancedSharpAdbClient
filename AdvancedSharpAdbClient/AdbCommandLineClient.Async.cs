#if HAS_TASK
// <copyright file="AdbCommandLineClient.Async.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using AdvancedSharpAdbClient.Exceptions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;

namespace AdvancedSharpAdbClient
{
    public partial class AdbCommandLineClient
    {
        /// <summary>
        /// Queries adb for its version number and checks it against <see cref="AdbServer.RequiredAdbVersion"/>.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which return a <see cref="Version"/> object that contains the version number of the Android Command Line client.</returns>
        public virtual async Task<Version> GetVersionAsync(CancellationToken cancellationToken = default)
        {
            // Run the adb.exe version command and capture the output.
            List<string> standardOutput = new();

            await RunAdbProcessAsync("version", null, standardOutput, cancellationToken);

            // Parse the output to get the version.
            Version version = GetVersionFromOutput(standardOutput) ?? throw new AdbException($"The version of the adb executable at {AdbPath} could not be determined.");

            if (version < AdbServer.RequiredAdbVersion)
            {
                AdbException ex = new($"Required minimum version of adb: {AdbServer.RequiredAdbVersion}. Current version is {version}");
#if HAS_LOGGER
                logger.LogError(ex, ex.Message);
#endif
                throw ex;
            }

            return version;
        }

        /// <summary>
        /// Starts the adb server by running the <c>adb start-server</c> command.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public virtual async Task StartServerAsync(CancellationToken cancellationToken = default)
        {
            int status = await RunAdbProcessInnerAsync("start-server", null, null, cancellationToken);

            if (status == 0)
            {
                return;
            }

#if HAS_PROCESS && !WINDOWS_UWP
            try
            {
                // Starting the adb server failed for whatever reason. This can happen if adb.exe
                // is running but is not accepting requests. In that case, try to kill it & start again.
                // It kills all processes named "adb", so let's hope nobody else named their process that way.
                foreach (Process adbProcess in Process.GetProcessesByName("adb"))
                {
                    try
                    {
                        adbProcess.Kill();
                    }
                    catch (Win32Exception)
                    {
                        // The associated process could not be terminated
                        // or
                        // The process is terminating.
                    }
                    catch (InvalidOperationException)
                    {
                        // The process has already exited.
                        // There is no process associated with this Process object.
                    }
                }
            }
            catch (NotSupportedException)
            {
                // This platform does not support getting a list of processes.
            }
#endif

            // Try again. This time, we don't call "Inner", and an exception will be thrown if the start operation fails
            // again. We'll let that exception bubble up the stack.
            await RunAdbProcessAsync("start-server", null, null, cancellationToken);
        }

        /// <summary>
        /// Runs the <c>adb.exe</c> process, invoking a specific <paramref name="command"/>,
        /// and reads the standard output and standard error output.
        /// </summary>
        /// <param name="command">The <c>adb.exe</c> command to invoke, such as <c>version</c> or <c>start-server</c>.</param>
        /// <param name="errorOutput">A list in which to store the standard error output. Each line is added as a new entry.
        /// This value can be <see langword="null"/> if you are not interested in the standard error.</param>
        /// <param name="standardOutput">A list in which to store the standard output. Each line is added as a new entry.
        /// This value can be <see langword="null"/> if you are not interested in the standard output.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        /// <remarks>Use this command only for <c>adb</c> commands that return immediately, such as
        /// <c>adb version</c>. This operation times out after 5 seconds.</remarks>
        /// <exception cref="AdbException">The process exited with an exit code other than <c>0</c>.</exception>
        protected virtual async Task RunAdbProcessAsync(string command, List<string> errorOutput, List<string> standardOutput, CancellationToken cancellationToken = default)
        {
            int status = await RunAdbProcessInnerAsync(command, errorOutput, standardOutput, cancellationToken);

            if (status != 0)
            {
                throw new AdbException($"The adb process returned error code {status} when running command {command}");
            }
        }

        /// <summary>
        /// Runs the <c>adb.exe</c> process, invoking a specific <paramref name="command"/>,
        /// and reads the standard output and standard error output.
        /// </summary>
        /// <param name="command">The <c>adb.exe</c> command to invoke, such as <c>version</c> or <c>start-server</c>.</param>
        /// <param name="errorOutput">A list in which to store the standard error output. Each line is added as a new entry.
        /// This value can be <see langword="null"/> if you are not interested in the standard error.</param>
        /// <param name="standardOutput">A list in which to store the standard output. Each line is added as a new entry.
        /// This value can be <see langword="null"/> if you are not interested in the standard output.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which return the return code of the <c>adb</c> process.</returns>
        /// <remarks>Use this command only for <c>adb</c> commands that return immediately, such as
        /// <c>adb version</c>. This operation times out after 5 seconds.</remarks>
        protected virtual async Task<int> RunAdbProcessInnerAsync(string command, List<string> errorOutput, List<string> standardOutput, CancellationToken cancellationToken = default)
        {
            ExceptionExtensions.ThrowIfNull(command);

            int status = await CrossPlatformFunc.RunProcessAsync(AdbPath, command, errorOutput, standardOutput, cancellationToken);

            return status;
        }
    }
}
#endif