// <copyright file="IAdbCommandLineClient.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;

namespace AdvancedSharpAdbClient
{
    /// <summary>
    /// Provides a common interface for any class that provides access to the <c>adb.exe</c> executable.
    /// </summary>
    public partial interface IAdbCommandLineClient
    {
        /// <summary>
        /// Queries adb for its version number and checks it against <see cref="AdbServer.RequiredAdbVersion"/>.
        /// </summary>
        Version GetVersion();

        /// <summary>
        /// Starts the adb server by running the <c>adb start-server</c> command.
        /// </summary>
        void StartServer();

        /// <summary>
        /// Determines whether the <c>adb.exe</c> file exists.
        /// </summary>
        /// <param name="adbPath">The path to validate.</param>
        bool CheckFileExists(string adbPath);
    }
}
