// <copyright file="VersionInfo.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

namespace AdvancedSharpAdbClient.DeviceCommands
{
    /// <summary>
    /// Represents a version of an Android application.
    /// </summary>
    public class VersionInfo
    {
        /// <summary>
        /// Gets or sets the version code of an Android application.
        /// </summary>
        public int VersionCode { get; set; }

        /// <summary>
        /// Gets or sets the version name of an Android application.
        /// </summary>
        public string VersionName { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionInfo"/> class.
        /// </summary>
        /// <param name="versionCode">The version code of the application.</param>
        /// <param name="versionName">The version name of the application.</param>
        public VersionInfo(int versionCode, string versionName)
        {
            VersionCode = versionCode;
            VersionName = versionName;
        }

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
        public override string ToString() => $"{VersionName} - {VersionCode}";
    }
}
