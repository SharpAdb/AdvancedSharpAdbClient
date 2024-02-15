// <copyright file="AdbCommandStatus.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace AdvancedSharpAdbClient.Models
{
    /// <summary>
    /// The version and path of the adb command line client.
    /// </summary>
#if HAS_BUFFERS
    [CollectionBuilder(typeof(EnumerableBuilder), nameof(EnumerableBuilder.AdbCommandLineStatusCreator))]
#endif
    [DebuggerDisplay($"{nameof(AdbServerStatus)} \\{{ {nameof(AdbVersion)} = {{{nameof(AdbVersion)}}}, {nameof(FileVersion)} = {{{nameof(FileVersion)}}}, {nameof(FilePath)} = {{{nameof(FilePath)}}} }}")]
    public readonly partial record struct AdbCommandLineStatus(Version? AdbVersion, string? FileVersion, string? FilePath)
    {
        /// <summary>
        /// The regex pattern for getting the adb version from the <c>adb version</c> command.
        /// </summary>
        private const string VersionPattern = @"^.*(\d+)\.(\d+)\.(\d+)$";

        /// <summary>
        /// Gets the version of the adb server.
        /// </summary>
        public Version? AdbVersion { get; init; } = AdbVersion;

        /// <summary>
        /// Gets the version of the adb command line client.
        /// </summary>
        public string? FileVersion { get; init; } = FileVersion;

        /// <summary>
        /// Gets the path to the adb command line client.
        /// </summary>
        public string? FilePath { get; init; } = FilePath;

        /// <summary>
        /// Parses the output of the <c>adb.exe version</c> command and determines the adb version.
        /// </summary>
        /// <param name="output">The output of the <c>adb.exe version</c> command.</param>
        /// <returns>A <see cref="AdbCommandLineStatus"/> object that represents the version and path of the adb command line client.</returns>
        public static AdbCommandLineStatus GetVersionFromOutput(IEnumerable<string> output)
        {
            int index = 0;
            Version? adbVersion = null;
            string? fileVersion = null, filePath = null;
            using IEnumerator<string> enumerator = output.GetEnumerator();
            while (index < 3 && enumerator.MoveNext())
            {
                string line = enumerator.Current;

                // Skip empty lines
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }

                switch (index)
                {
                    case 0:
                        Match matcher = VersionRegex().Match(line);
                        if (matcher.Success)
                        {
                            int majorVersion = int.Parse(matcher.Groups[1].Value);
                            int minorVersion = int.Parse(matcher.Groups[2].Value);
                            int microVersion = int.Parse(matcher.Groups[3].Value);

                            adbVersion = new Version(majorVersion, minorVersion, microVersion);
                        }
                        break;
                    case 1:
                        fileVersion = line.Split(' ', StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
                        break;
                    case 2 when line.StartsWith("Installed as "):
                        filePath = line[13..];
                        break;
                }
                index++;
            }
            return new(adbVersion, fileVersion, filePath);
        }

#if HAS_BUFFERS
        /// <summary>
        /// Parses the output of the <c>adb.exe version</c> command and determines the adb version.
        /// </summary>
        /// <param name="output">The output of the <c>adb.exe version</c> command.</param>
        /// <returns>A <see cref="AdbCommandLineStatus"/> object that represents the version and path of the adb command line client.</returns>
        public static AdbCommandLineStatus GetVersionFromOutput(ReadOnlySpan<string> output)
        {
            int index = 0;
            Version? adbVersion = null;
            string? fileVersion = null, filePath = null;
            ReadOnlySpan<string>.Enumerator enumerator = output.GetEnumerator();
            while (index < 3 && enumerator.MoveNext())
            {
                string line = enumerator.Current;

                // Skip empty lines
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }

                switch (index)
                {
                    case 0:
                        Match matcher = VersionRegex().Match(line);
                        if (matcher.Success)
                        {
                            int majorVersion = int.Parse(matcher.Groups[1].Value);
                            int minorVersion = int.Parse(matcher.Groups[2].Value);
                            int microVersion = int.Parse(matcher.Groups[3].Value);

                            adbVersion = new Version(majorVersion, minorVersion, microVersion);
                        }
                        break;
                    case 1:
                        fileVersion = line.Split(' ', StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
                        break;
                    case 2 when line.StartsWith("Installed as "):
                        filePath = line[13..];
                        break;
                }
                index++;
            }
            return new(adbVersion, fileVersion, filePath);
        }
#endif

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="AdbCommandLineStatus"/>.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the <see cref="AdbCommandLineStatus"/>.</returns>
        public readonly IEnumerator<string> GetEnumerator()
        {
            if (AdbVersion == null) { yield break; }
            yield return $"Android Debug Bridge version {AdbVersion}";
            if (string.IsNullOrEmpty(FileVersion)) { yield break; }
            yield return $"Version {FileVersion}";
            if (string.IsNullOrEmpty(FilePath)) { yield break; }
            yield return $"Installed as {FilePath}";
        }

        /// <inheritdoc/>
        public override string ToString() => string.Join(Environment.NewLine, [.. this]);

#if NET7_0_OR_GREATER
        [GeneratedRegex(VersionPattern)]
        private static partial Regex VersionRegex();
#else
        /// <summary>
        /// Gets a <see cref="Regex"/> for parsing the adb version.
        /// </summary>
        /// <returns>The <see cref="Regex"/> for parsing the adb version.</returns>
        private static Regex VersionRegex() => new(VersionPattern);
#endif
    }
}
