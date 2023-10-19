// <copyright file="AdbCommandLineClient.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using AdvancedSharpAdbClient.Exceptions;
using AdvancedSharpAdbClient.Logs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.RegularExpressions;

namespace AdvancedSharpAdbClient
{
    /// <summary>
    /// Provides methods for interacting with the <c>adb.exe</c> command line client.
    /// </summary>
    public partial class AdbCommandLineClient : IAdbCommandLineClient
    {
        /// <summary>
        /// The regex pattern for getting the adb version from the <c>adb version</c> command.
        /// </summary>
        protected const string AdbVersionPattern = "^.*(\\d+)\\.(\\d+)\\.(\\d+)$";

        private static readonly char[] separator = Extensions.NewLineSeparator;

        /// <summary>
        /// The logger to use when logging messages.
        /// </summary>
        protected readonly ILogger<AdbCommandLineClient> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdbCommandLineClient"/> class.
        /// </summary>
        /// <param name="adbPath">The path to the <c>adb.exe</c> executable.</param>
        /// <param name="isForce">Don't check adb file name when <see langword="true"/>.</param>
        /// <param name="logger">The logger to use when logging.</param>
        public AdbCommandLineClient(string adbPath, bool isForce = false, ILogger<AdbCommandLineClient> logger = null)
        {
            if (StringExtensions.IsNullOrWhiteSpace(adbPath))
            {
                throw new ArgumentNullException(nameof(adbPath));
            }

            if (!isForce)
            {
                EnsureIsValidAdbFile(adbPath);
            }

            if (!CheckFileExists(adbPath))
            {
                throw new FileNotFoundException($"The adb.exe executable could not be found at {adbPath}");
            }

            AdbPath = adbPath;
            this.logger = logger ?? LoggerProvider.CreateLogger<AdbCommandLineClient>();
        }

        /// <summary>
        /// Gets the path to the <c>adb.exe</c> executable.
        /// </summary>
        public string AdbPath { get; protected set; }

        /// <summary>
        /// Queries adb for its version number and checks it against <see cref="AdbServer.RequiredAdbVersion"/>.
        /// </summary>
        /// <returns>A <see cref="Version"/> object that contains the version number of the Android Command Line client.</returns>
        public virtual Version GetVersion()
        {
            // Run the adb.exe version command and capture the output.
            List<string> standardOutput = [];

            RunAdbProcess("version", null, standardOutput);

            // Parse the output to get the version.
            Version version = GetVersionFromOutput(standardOutput) ?? throw new AdbException($"The version of the adb executable at {AdbPath} could not be determined.");

            if (version < AdbServer.RequiredAdbVersion)
            {
                AdbException ex = new($"Required minimum version of adb: {AdbServer.RequiredAdbVersion}. Current version is {version}");
                logger.LogError(ex, ex.Message);
                throw ex;
            }

            return version;
        }

        /// <summary>
        /// Starts the adb server by running the <c>adb start-server</c> command.
        /// </summary>
        public virtual void StartServer()
        {
            int status = RunAdbProcessInner("start-server", null, null);

            if (status == 0)
            {
                return;
            }

#if HAS_PROCESS && !WINDOWS_UWP
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
#endif

            // Try again. This time, we don't call "Inner", and an exception will be thrown if the start operation fails
            // again. We'll let that exception bubble up the stack.
            RunAdbProcess("start-server", null, null);
        }

        /// <inheritdoc/>
        public virtual bool CheckFileExists(string adbPath) => Factories.CheckFileExists(adbPath);

        /// <summary>
        /// Throws an error if the path does not point to a valid instance of <c>adb.exe</c>.
        /// </summary>
        /// <param name="adbPath">The path to validate.</param>
        protected virtual void EnsureIsValidAdbFile(string adbPath)
        {
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
        /// Parses the output of the <c>adb.exe version</c> command and determines the adb version.
        /// </summary>
        /// <param name="output">The output of the <c>adb.exe version</c> command.</param>
        /// <returns>A <see cref="Version"/> object that represents the version of the adb command line client.</returns>
        protected static Version GetVersionFromOutput(IEnumerable<string> output)
        {
            Regex regex = AdbVersionRegex();
            foreach (string line in output)
            {
                // Skip empty lines
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }

                Match matcher = regex.Match(line);
                if (matcher.Success)
                {
                    int majorVersion = int.Parse(matcher.Groups[1].Value);
                    int minorVersion = int.Parse(matcher.Groups[2].Value);
                    int microVersion = int.Parse(matcher.Groups[3].Value);

                    return new Version(majorVersion, minorVersion, microVersion);
                }
            }

            return null;
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
        /// <remarks>Use this command only for <c>adb</c> commands that return immediately, such as
        /// <c>adb version</c>. This operation times out after 5 seconds.</remarks>
        /// <exception cref="AdbException">The process exited with an exit code other than <c>0</c>.</exception>
        protected virtual void RunAdbProcess(string command, ICollection<string> errorOutput, ICollection<string> standardOutput)
        {
            int status = RunAdbProcessInner(command, errorOutput, standardOutput);

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
        /// <returns>The return code of the <c>adb</c> process.</returns>
        /// <remarks>Use this command only for <c>adb</c> commands that return immediately, such as
        /// <c>adb version</c>. This operation times out after 5 seconds.</remarks>
        protected virtual int RunAdbProcessInner(string command, ICollection<string> errorOutput, ICollection<string> standardOutput)
        {
            ExceptionExtensions.ThrowIfNull(command);

            int status = RunProcess(AdbPath, command, errorOutput, standardOutput);

            return status;
        }

        /// <summary>
        /// Runs process, invoking a specific command, and reads the standard output and standard error output.
        /// </summary>
        /// <returns>The return code of the process.</returns>
#if !HAS_PROCESS
        [DoesNotReturn]
#endif
        protected virtual int RunProcess(string filename, string command, ICollection<string> errorOutput, ICollection<string> standardOutput)
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
        }

#if NET7_0_OR_GREATER
        [GeneratedRegex(AdbVersionPattern)]
        private static partial Regex AdbVersionRegex();
#else
        private static Regex AdbVersionRegex() => new(AdbVersionPattern);
#endif
    }
}
