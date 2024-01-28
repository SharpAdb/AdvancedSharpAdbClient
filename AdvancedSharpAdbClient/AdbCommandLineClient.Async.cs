#if HAS_TASK
// <copyright file="AdbCommandLineClient.Async.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;

namespace AdvancedSharpAdbClient
{
    public partial class AdbCommandLineClient
    {
        /// <inheritdoc/>
        public virtual async Task<AdbCommandLineStatus> GetVersionAsync(CancellationToken cancellationToken = default)
        {
            // Run the adb.exe version command and capture the output.
            List<string> standardOutput = [];
            await RunAdbProcessAsync("version", null, standardOutput, cancellationToken).ConfigureAwait(false);

            // Parse the output to get the version.
            AdbCommandLineStatus version = AdbCommandLineStatus.GetVersionFromOutput(standardOutput);

            if (version.AdbVersion == null)
            {
                throw new AdbException($"The version of the adb executable at {AdbPath} could not be determined.");
            }
            else if (version.AdbVersion < AdbServer.RequiredAdbVersion)
            {
                AdbException ex = new($"Required minimum version of adb: {AdbServer.RequiredAdbVersion}. Current version is {version}");
                logger.LogError(ex, ex.Message);
                throw ex;
            }

            return version;
        }

        /// <inheritdoc/>
        public virtual async Task StartServerAsync(CancellationToken cancellationToken = default)
        {
            int status = await RunAdbProcessInnerAsync("start-server", null, null, cancellationToken).ConfigureAwait(false);
            if (status == 0) { return; }
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
            await RunAdbProcessAsync("start-server", null, null, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public virtual Task<bool> CheckAdbFileExistsAsync(string adbPath, CancellationToken cancellationToken = default) => adbPath == "adb" ? TaskExExtensions.FromResult(true) :
#if WINDOWS_UWP
            StorageFile.GetFileFromPathAsync(adbPath).AsTask(cancellationToken).ContinueWith(x => x.Result != null && x.Result.IsOfType(StorageItemTypes.File));
#else
            TaskExExtensions.FromResult(File.Exists(adbPath));
#endif

        /// <summary>
        /// Asynchronously runs the <c>adb.exe</c> process, invoking a specific <paramref name="command"/>,
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
        protected virtual async Task RunAdbProcessAsync(string command, ICollection<string>? errorOutput, ICollection<string>? standardOutput, CancellationToken cancellationToken = default)
        {
            int status = await RunAdbProcessInnerAsync(command, errorOutput, standardOutput, cancellationToken).ConfigureAwait(false);
            if (status != 0) { throw new AdbException($"The adb process returned error code {status} when running command {command}"); }
        }

        /// <summary>
        /// Asynchronously runs the <c>adb.exe</c> process, invoking a specific <paramref name="command"/>,
        /// and reads the standard output and standard error output.
        /// </summary>
        /// <param name="command">The <c>adb.exe</c> command to invoke, such as <c>version</c> or <c>start-server</c>.</param>
        /// <param name="errorOutput">A list in which to store the standard error output. Each line is added as a new entry.
        /// This value can be <see langword="null"/> if you are not interested in the standard error.</param>
        /// <param name="standardOutput">A list in which to store the standard output. Each line is added as a new entry.
        /// This value can be <see langword="null"/> if you are not interested in the standard output.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task{Int32}"/> which returns the return code of the <c>adb</c> process.</returns>
        /// <remarks>Use this command only for <c>adb</c> commands that return immediately, such as
        /// <c>adb version</c>. This operation times out after 5 seconds.</remarks>
        protected virtual async Task<int> RunAdbProcessInnerAsync(string command, ICollection<string>? errorOutput, ICollection<string>? standardOutput, CancellationToken cancellationToken = default)
        {
            ExceptionExtensions.ThrowIfNull(command);
            return await RunProcessAsync(AdbPath, command, errorOutput, standardOutput, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously runs process, invoking a specific command, and reads the standard output and standard error output.
        /// </summary>
        /// <returns>The return code of the process.</returns>
#if !HAS_PROCESS
        [DoesNotReturn]
#endif
        protected virtual async Task<int> RunProcessAsync(string filename, string command, ICollection<string>? errorOutput, ICollection<string>? standardOutput, CancellationToken cancellationToken = default)
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

            using Process process = Process.Start(psi) ?? throw new AdbException($"The adb process could not be started. The process returned null when starting {filename} {command}");

            string standardErrorString = await process.StandardError.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
            string standardOutputString = await process.StandardOutput.ReadToEndAsync(cancellationToken).ConfigureAwait(false);

            errorOutput?.AddRange(standardErrorString.Split(separator, StringSplitOptions.RemoveEmptyEntries));
            standardOutput?.AddRange(standardOutputString.Split(separator, StringSplitOptions.RemoveEmptyEntries));

#if NET5_0_OR_GREATER
            using (CancellationTokenSource completionSource = new(TimeSpan.FromMilliseconds(5000)))
            {
                try
                {
                    await process.WaitForExitAsync(completionSource.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (completionSource.IsCancellationRequested)
                {
                    if (!process.HasExited)
                    {
                        process.Kill();
                    }
                }
            }
#else
            if (!process.WaitForExit(5000))
            {
                process.Kill();
            }
#endif
            // get the return code from the process
            return process.ExitCode;
#else
            await Task.CompletedTask;
            throw new PlatformNotSupportedException("This platform is not support System.Diagnostics.Process. You can start adb server by running `adb start-server` manually.");
#endif
        }
    }
}
#endif