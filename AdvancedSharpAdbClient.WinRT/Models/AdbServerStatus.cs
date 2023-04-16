// <copyright file="AdbServerStatus.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using AdvancedSharpAdbClient.WinRT.Extensions;
using Windows.ApplicationModel;

namespace AdvancedSharpAdbClient.WinRT
{
    /// <summary>
    /// Represents the status of the adb server.
    /// </summary>
    public sealed class AdbServerStatus
    {
        internal AdvancedSharpAdbClient.AdbServerStatus adbServerStatus;

        /// <summary>
        /// Gets or sets a value indicating whether the server is currently running.
        /// </summary>
        public bool IsRunning
        {
            get => adbServerStatus.IsRunning;
            set => adbServerStatus.IsRunning = value;
        }

        /// <summary>
        /// Gets or sets, when the server is running, the version of the server that is running.
        /// </summary>
        public PackageVersion Version
        {
            get => adbServerStatus.Version.GetPackageVersion();
            set => adbServerStatus.Version = value.GetVersion();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdbServerStatus"/> class.
        /// </summary>
        public AdbServerStatus() => adbServerStatus = new();

        internal AdbServerStatus(AdvancedSharpAdbClient.AdbServerStatus adbServerStatus) => this.adbServerStatus = adbServerStatus;

        internal static AdbServerStatus GetAdbServerStatus(AdvancedSharpAdbClient.AdbServerStatus adbServerStatus) => new(adbServerStatus);

        /// <summary>
        /// Gets a <see cref="string"/> that represents the current <see cref="AdbServerStatus"/> object.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents the current <see cref="AdbServerStatus"/> object.</returns>
        public override string ToString() => adbServerStatus.ToString();
    }
}
