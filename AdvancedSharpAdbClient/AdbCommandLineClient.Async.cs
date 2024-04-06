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
using System.Security.Cryptography;
using System.Threading;

namespace AdvancedSharpAdbClient
{
    public partial class AdbCommandLineClient
    {
        /// <inheritdoc/>
        public async Task<AdbCommandLineStatus> GetVersionAsync(CancellationToken cancellationToken = default)
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
        public virtual async Task<List<string>> ExecuteAdbCommandAsync(string command, CancellationToken cancellationToken = default)
        {
            List<string> errorOutput = [];
            List<string> standardOutput = [];
            int status = await RunAdbProcessInnerAsync(command, errorOutput, standardOutput, cancellationToken).ConfigureAwait(false);
            if (errorOutput.Count > 0)
            {
                string error = StringExtensions.Join(Environment.NewLine, errorOutput!);
                throw new AdbException($"The adb process returned error code {status} when running command {command} with error output:{Environment.NewLine}{error}", error);
            }
            else
            {
                return status != 0
                ? throw new AdbException($"The adb process returned error code {status} when running command {command}")
                : standardOutput;
            }
        }

        /// <inheritdoc/>
        public async Task StartServerAsync(CancellationToken cancellationToken = default)
        {
            int status = await RunAdbProcessInnerAsync("start-server", null, null, cancellationToken).ConfigureAwait(false);
            if (status == 0) { return; }

            // Starting the adb server failed for whatever reason. This can happen if adb.exe
            // is running but is not accepting requests. In that case, try to kill it & start again.
            // It kills all processes named "adb", so let's hope nobody else named their process that way.
            KillProcess("adb");

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
        /// <remarks>Use this command only for <c>adb</c> commands that return immediately, such as <c>adb version</c>.</remarks>
        /// <exception cref="AdbException">The process exited with an exit code other than <c>0</c>.</exception>
        protected async Task RunAdbProcessAsync(string command, ICollection<string>? errorOutput, ICollection<string>? standardOutput, CancellationToken cancellationToken = default)
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
        /// <remarks>Use this command only for <c>adb</c> commands that return immediately, such as <c>adb version</c>.</remarks>
        protected async Task<int> RunAdbProcessInnerAsync(string command, ICollection<string>? errorOutput, ICollection<string>? standardOutput, CancellationToken cancellationToken = default)
        {
            ExceptionExtensions.ThrowIfNull(command);
            return await RunProcessAsync(AdbPath, command, errorOutput, standardOutput, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously runs process, invoking a specific command, and reads the standard output and standard error output.
        /// </summary>
        /// <param name="filename">The filename of the process to start.</param>
        /// <param name="command">The command to invoke, such as <c>version</c> or <c>start-server</c>.</param>
        /// <param name="errorOutput">A list in which to store the standard error output. Each line is added as a new entry.
        /// This value can be <see langword="null"/> if you are not interested in the standard error.</param>
        /// <param name="standardOutput">A list in which to store the standard output. Each line is added as a new entry.
        /// This value can be <see langword="null"/> if you are not interested in the standard output.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task{Int32}"/> which returns the return code of the process.</returns>
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

            using (CancellationTokenRegistration registration = cancellationToken.Register(process.Kill))
            {
                string standardErrorString = await process.StandardError.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
                string standardOutputString = await process.StandardOutput.ReadToEndAsync(cancellationToken).ConfigureAwait(false);

                errorOutput?.AddRange(standardErrorString.Split(separator, StringSplitOptions.RemoveEmptyEntries));
                standardOutput?.AddRange(standardOutputString.Split(separator, StringSplitOptions.RemoveEmptyEntries));
            }

            if (!process.HasExited)
            {
                process.Kill();
            }

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