// <copyright file="IAdbCommandLineClient.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

namespace AdvancedSharpAdbClient
{
    /// <summary>
    /// Provides a common interface for any class that provides access to the <c>adb.exe</c> executable.
    /// </summary>
    public partial interface IAdbCommandLineClient
    {
        /// <summary>
        /// Queries adb for its version and path and checks it against <see cref="AdbServer.RequiredAdbVersion"/>.
        /// </summary>
        /// <returns>A <see cref="AdbCommandLineStatus"/> object that represents the version and path of the adb command line client.</returns>
        AdbCommandLineStatus GetVersion();

        /// <summary>
        /// Starts the adb server by running the <c>adb start-server</c> command.
        /// </summary>
        void StartServer();

        /// <summary>
        /// Determines whether the <c>adb.exe</c> file exists.
        /// </summary>
        /// <param name="adbPath">The path to validate.</param>
        /// <returns><see langword="true"/> if the <c>adb.exe</c> file is exists, otherwise <see langword="false"/>.</returns>
        bool CheckAdbFileExists(string adbPath);
    }
}
