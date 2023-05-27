// <copyright file="AdbCommandLineClient.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using AdvancedSharpAdbClient.Exceptions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

#if HAS_LOGGER
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
#endif

namespace AdvancedSharpAdbClient
{
    /// <summary>
    /// Provides methods for interacting with the <c>adb.exe</c> command line client.
    /// </summary>
    public class AdbCommandLineClient : IAdbCommandLineClient
    {
        /// <summary>
        /// The regex pattern for getting the adb version from the <c>adb version</c> command.
        /// </summary>
        private const string AdbVersionPattern = "^.*(\\d+)\\.(\\d+)\\.(\\d+)$";

#if HAS_LOGGER
        /// <summary>
        /// The logger to use when logging messages.
        /// </summary>
        private readonly ILogger<AdbCommandLineClient> logger;
#endif

#if !HAS_LOGGER
#pragma warning disable CS1572 // XML 注释中有 param 标记，但是没有该名称的参数
#endif
        /// <summary>
        /// Initializes a new instance of the <see cref="AdbCommandLineClient"/> class.
        /// </summary>
        /// <param name="adbPath">The path to the <c>adb.exe</c> executable.</param>
        /// <param name="logger">The logger to use when logging.</param>
        public AdbCommandLineClient(string adbPath
#if HAS_LOGGER
            , ILogger<AdbCommandLineClient> logger = null
#endif
            )
        {
            if (adbPath.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException(nameof(adbPath));
            }

            bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            bool isUnix = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

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

            this.EnsureIsValidAdbFile(adbPath);

            AdbPath = adbPath;
#if HAS_LOGGER
            this.logger = logger ?? NullLogger<AdbCommandLineClient>.Instance;
#endif
        }
#if !HAS_LOGGER
#pragma warning restore CS1572 // XML 注释中有 param 标记，但是没有该名称的参数
#endif

        /// <summary>
        /// Gets the path to the <c>adb.exe</c> executable.
        /// </summary>
        public string AdbPath { get; private set; }

        /// <summary>
        /// Queries adb for its version number and checks it against <see cref="AdbServer.RequiredAdbVersion"/>.
        /// </summary>
        /// <returns>A <see cref="Version"/> object that contains the version number of the Android Command Line client.</returns>
        public Version GetVersion()
        {
            // Run the adb.exe version command and capture the output.
            List<string> standardOutput = new();

            RunAdbProcess("version", null, standardOutput);

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
        public void StartServer()
        {
            int status = RunAdbProcessInner("start-server", null, null);

            if (status == 0)
            {
                return;
            }

#if HAS_PROCESS
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
        public virtual bool IsValidAdbFile(string adbPath) => CrossPlatformFunc.CheckFileExists(adbPath);

        /// <summary>
        /// Parses the output of the <c>adb.exe version</c> command and determines the adb version.
        /// </summary>
        /// <param name="output">The output of the <c>adb.exe version</c> command.</param>
        /// <returns>A <see cref="Version"/> object that represents the version of the adb command line client.</returns>
        internal static Version GetVersionFromOutput(IEnumerable<string> output)
        {
            foreach (string line in output)
            {
                // Skip empty lines
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }

                Match matcher = Regex.Match(line, AdbVersionPattern);
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
        protected virtual void RunAdbProcess(string command, List<string> errorOutput, List<string> standardOutput)
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
        protected virtual int RunAdbProcessInner(string command, List<string> errorOutput, List<string> standardOutput)
        {
            ExceptionExtensions.ThrowIfNull(command);

            int status = CrossPlatformFunc.RunProcess(AdbPath, command, errorOutput, standardOutput);

            return status;
        }
    }
}
