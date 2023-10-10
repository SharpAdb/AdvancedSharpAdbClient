// <copyright file="InstallProgressEventArgs.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;

namespace AdvancedSharpAdbClient.DeviceCommands
{
    /// <summary>
    /// Represents the state of the installation for <see cref="PackageManager.InstallProgressChanged"/>.
    /// </summary>
    public class InstallProgressEventArgs(PackageInstallProgressState state) : EventArgs
    {
        /// <summary>
        /// State of the installation.
        /// </summary>
        public PackageInstallProgressState State { get; } = state;

        /// <summary>
        /// Number of packages which is finished operation.
        /// Used only in <see cref="PackageInstallProgressState.Uploading"/>,
        /// <see cref="PackageInstallProgressState.WriteSession"/> and
        /// <see cref="PackageInstallProgressState.PostInstall"/> state.
        /// </summary>
        public int PackageFinished { get; init; }

        /// <summary>
        /// Number of packages required for this operation.
        /// Used only in <see cref="PackageInstallProgressState.Uploading"/>,
        /// <see cref="PackageInstallProgressState.WriteSession"/> and
        /// <see cref="PackageInstallProgressState.PostInstall"/> state.
        /// </summary>
        public int PackageRequired { get; init; }

        /// <summary>
        /// Upload percentage completed.
        /// Used only in <see cref="PackageInstallProgressState.Uploading"/> state.
        /// </summary>
        public double UploadProgress { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InstallProgressEventArgs"/> class.
        /// Which is used for <see cref="PackageInstallProgressState.Uploading"/> state.
        /// </summary>
        public InstallProgressEventArgs(int packageUploaded, int packageRequired, double uploadProgress) : this(PackageInstallProgressState.Uploading)
        {
            PackageFinished = packageUploaded;
            PackageRequired = packageRequired;
            UploadProgress = uploadProgress;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InstallProgressEventArgs"/> class.
        /// Which is used for <see cref="PackageInstallProgressState.Uploading"/> and <see cref="PackageInstallProgressState.WriteSession"/> state.
        /// </summary>
        public InstallProgressEventArgs(int packageCleaned, int packageRequired, PackageInstallProgressState state) : this(state)
        {
            PackageFinished = packageCleaned;
            PackageRequired = packageRequired;
        }
    }
}
