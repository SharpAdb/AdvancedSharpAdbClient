// <copyright file="AdbCommandLineClient.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
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
    /// <summary>
    /// Provides methods for interacting with the <c>adb.exe</c> command line client.
    /// </summary>
    [DebuggerDisplay($"{nameof(AdbCommandLineClient)} \\{{ {nameof(AdbPath)} = {{{nameof(AdbPath)}}} }}")]
    public partial class AdbCommandLineClient : IAdbCommandLineClient
    {
#if HAS_PROCESS
        /// <summary>
        /// The <see cref="Array"/> of <see cref="char"/>s that represent a new line.
        /// </summary>
        private static readonly char[] separator = Extensions.NewLineSeparator;
#endif

        /// <summary>
        /// The logger to use when logging messages.
        /// </summary>
        private readonly ILogger<AdbCommandLineClient> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdbCommandLineClient"/> class.
        /// </summary>
        /// <param name="adbPath">The path to the <c>adb.exe</c> executable.</param>
        /// <param name="isForce">Doesn't check adb file when <see langword="true"/>.</param>
        /// <param name="logger">The logger to use when logging.</param>
        public AdbCommandLineClient(string adbPath, bool isForce = false, ILogger<AdbCommandLineClient>? logger = null)
        {
            if (StringExtensions.IsNullOrWhiteSpace(adbPath))
            {
                throw new ArgumentNullException(nameof(adbPath));
            }

            if (!isForce)
            {
                EnsureIsValidAdbFile(adbPath);
                if (!CheckAdbFileExists(adbPath))
                {
                    throw new FileNotFoundException($"The adb.exe executable could not be found at {adbPath}");
                }
            }

            AdbPath = adbPath;
            this.logger = logger ?? LoggerProvider.CreateLogger<AdbCommandLineClient>();
        }

        /// <summary>
        /// Gets the path to the <c>adb.exe</c> executable.
        /// </summary>
        public string AdbPath { get; init; }

        /// <inheritdoc/>
        public AdbCommandLineStatus GetVersion()
        {
            // Run the adb.exe version command and capture the output.
            List<string> standardOutput = [];
            RunAdbProcess("version", null, standardOutput);

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
        public void StartServer(int timeout = Timeout.Infinite)
        {
            int status = RunAdbProcessInner("start-server", null, null, timeout);
            if (status == 0) { return; }

            // Starting the adb server failed for whatever reason. This can happen if adb.exe
            // is running but is not accepting requests. In that case, try to kill it & start again.
            // It kills all processes named "adb", so let's hope nobody else named their process that way.
            KillProcess("adb");

            // Try again. This time, we don't call "Inner", and an exception will be thrown if the start operation fails
            // again. We'll let that exception bubble up the stack.
            RunAdbProcess("start-server", null, null, timeout);
        }

        /// <inheritdoc/>
        /// <remarks>Use this command only for <c>adb</c> commands that return immediately, such as
        /// <c>adb version</c>. This operation times out after 5 seconds.</remarks>
        public virtual List<string> ExecuteAdbCommand(string command, int timeout = 5000)
        {
            List<string> errorOutput = [];
            List<string> standardOutput = [];
            int status = RunAdbProcessInner(command, errorOutput, standardOutput, timeout);
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
        public virtual bool CheckAdbFileExists(string adbPath) => adbPath == "adb" ||
#if WINDOWS_UWP
            StorageFile.GetFileFromPathAsync(adbPath).AwaitByTaskCompleteSource() is StorageFile file && file.IsOfType(StorageItemTypes.File);
#else
            File.Exists(adbPath);
#endif

        /// <inheritdoc/>
        public override string ToString() => $"The {nameof(AdbCommandLineClient)} process with adb command line at {AdbPath}";

        /// <summary>
        /// Throws an error if the path does not point to a valid instance of <c>adb.exe</c>.
        /// </summary>
        /// <param name="adbPath">The path to validate.</param>
        protected virtual void EnsureIsValidAdbFile(string adbPath)
        {
            if (adbPath == "adb") { return; }

            bool isWindows = Extensions.IsWindowsPlatform();
            bool isUnix = Extensions.IsUnixPlatform();

            if (isWindows)
            {
                if (!string.Equals(Path.GetFileName(adbPath), "adb.exe", StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentOutOfRangeException(nameof(adbPath), $"{adbPath} does not seem to be a valid adb.exe executable. The path must end with `adb.exe`");
                }
            }
            else if (isUnix)
            {
                if (!string.Equals(Path.GetFileName(adbPath), "adb", StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentOutOfRangeException(nameof(adbPath), $"{adbPath} does not seem to be a valid adb executable. The path must end with `adb`");
                }
            }
            else
            {
                throw new NotSupportedException("SharpAdbClient only supports launching adb.exe on Windows, Mac OS and Linux");
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
        /// <param name="timeout">The timeout in milliseconds to wait for the <c>adb</c> process to exit.</param>
        /// <remarks>Use this command only for <c>adb</c> commands that return immediately, such as
        /// <c>adb version</c>. This operation times out after 5 seconds in default.</remarks>
        /// <exception cref="AdbException">The process exited with an exit code other than <c>0</c>.</exception>
        protected void RunAdbProcess(string command, ICollection<string>? errorOutput, ICollection<string>? standardOutput, int timeout = 5000)
        {
            int status = RunAdbProcessInner(command, errorOutput, standardOutput, timeout);
            if (status != 0) { throw new AdbException($"The adb process returned error code {status} when running command {command}"); }
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
        /// <param name="timeout">The timeout in milliseconds to wait for the <c>adb</c> process to exit.</param>
        /// <returns>The return code of the <c>adb</c> process.</returns>
        /// <remarks>Use this command only for <c>adb</c> commands that return immediately, such as
        /// <c>adb version</c>. This operation times out after 5 seconds in default.</remarks>
        protected int RunAdbProcessInner(string command, ICollection<string>? errorOutput, ICollection<string>? standardOutput, int timeout = 5000)
        {
            ExceptionExtensions.ThrowIfNull(command);
            return RunProcess(AdbPath, command, errorOutput, standardOutput, timeout);
        }

        /// <summary>
        /// Kills all processes with the specified name.
        /// </summary>
        /// <param name="processName">The name of the process to kill. </param>
        protected virtual void KillProcess(string processName)
        {
#if HAS_PROCESS && !WINDOWS_UWP
            try
            {
                foreach (Process adbProcess in Process.GetProcessesByName(processName))
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
        }

        /// <summary>
        /// Runs process, invoking a specific command, and reads the standard output and standard error output.
        /// </summary>
        /// <param name="filename">The filename of the process to start.</param>
        /// <param name="command">The command to invoke, such as <c>version</c> or <c>start-server</c>.</param>
        /// <param name="errorOutput">A list in which to store the standard error output. Each line is added as a new entry.
        /// This value can be <see langword="null"/> if you are not interested in the standard error.</param>
        /// <param name="standardOutput">A list in which to store the standard output. Each line is added as a new entry.
        /// This value can be <see langword="null"/> if you are not interested in the standard output.</param>
        /// <param name="timeout">The timeout in milliseconds to wait for the process to exit.</param>
        /// <returns>The return code of the process.</returns>
#if !HAS_PROCESS
        [DoesNotReturn]
#endif
        protected virtual int RunProcess(string filename, string command, ICollection<string>? errorOutput, ICollection<string>? standardOutput, int timeout)
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

            // get the return code from the process
            if (!process.WaitForExit(timeout))
            {
                process.Kill();
            }

            string standardErrorString = process.StandardError.ReadToEnd();
            string standardOutputString = process.StandardOutput.ReadToEnd();

            errorOutput?.AddRange(standardErrorString.Split(separator, StringSplitOptions.RemoveEmptyEntries));
            standardOutput?.AddRange(standardOutputString.Split(separator, StringSplitOptions.RemoveEmptyEntries));

            return process.ExitCode;
#else
            throw new PlatformNotSupportedException("This platform is not support System.Diagnostics.Process. You can start adb server by running `adb start-server` manually.");
#endif
        }
    }
}
