// <copyright file="VersionInfo.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

namespace AdvancedSharpAdbClient.DeviceCommands
{
    /// <summary>
    /// Represents a version of an Android application.
    /// </summary>
    /// <param name="versionCode">The version code of the application.</param>
    /// <param name="versionName">The version name of the application.</param>
    public class VersionInfo(int versionCode, string versionName)
    {
        /// <summary>
        /// Gets or sets the version code of an Android application.
        /// </summary>
        public int VersionCode { get; } = versionCode;

        /// <summary>
        /// Gets or sets the version name of an Android application.
        /// </summary>
        public string VersionName { get; } = versionName;

        /// <summary>
        /// Deconstruct the <see cref="VersionInfo"/> class.
        /// </summary>
        /// <param name="versionCode">The version code of the application.</param>
        /// <param name="versionName">The version name of the application.</param>
        public void Deconstruct(out int versionCode, out string versionName)
        {
            versionCode = VersionCode;
            versionName = VersionName;
        }

        /// <inheritdoc/>
        public override string ToString() => $"{VersionName} ({VersionCode})";
    }
}
