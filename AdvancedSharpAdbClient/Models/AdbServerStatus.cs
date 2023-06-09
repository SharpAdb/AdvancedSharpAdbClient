// <copyright file="AdbServerStatus.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;

namespace AdvancedSharpAdbClient
{
    /// <summary>
    /// Represents the status of the adb server.
    /// </summary>
    public struct AdbServerStatus
    {
        /// <summary>
        /// Gets a value indicating whether the server is currently running.
        /// </summary>
        public bool IsRunning { get; internal set; }

        /// <summary>
        /// Gets the version of the server when it is running.
        /// </summary>
        public Version Version { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdbServerStatus"/> struct.
        /// </summary>
        /// <param name="isRunning">The value indicating whether the server is currently running.</param>
        /// <param name="version">The version of the server when it is running.</param>
        public AdbServerStatus(bool isRunning, Version version)
        {
            IsRunning = isRunning;
            Version = version;
        }

        /// <summary>
        /// Deconstruct the <see cref="AdbServerStatus"/> struct.
        /// </summary>
        /// <param name="isRunning">The value indicating whether the server is currently running.</param>
        /// <param name="version">The version of the server when it is running.</param>
        public readonly void Deconstruct(out bool isRunning, out Version version)
        {
            isRunning = IsRunning;
            version = Version;
        }

        /// <summary>
        /// Gets a <see cref="string"/> that represents the current <see cref="AdbServerStatus"/> object.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents the current <see cref="AdbServerStatus"/> object.</returns>
        public override readonly string ToString() =>
            IsRunning ? $"Version {Version} of the adb daemon is running." : "The adb daemon is not running.";
    }
}
