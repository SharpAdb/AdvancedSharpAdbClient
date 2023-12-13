// <copyright file="InstallProgress.EventArgs.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;

namespace AdvancedSharpAdbClient.Models
{
    /// <summary>
    /// Represents the state of apk installation.
    /// </summary>
    /// <param name="state">The state of the installation.</param>
    public class InstallProgressEventArgs(PackageInstallProgressState state) : EventArgs
    {
        /// <summary>
        /// Gets the state of the installation.
        /// </summary>
        public PackageInstallProgressState State { get; } = state;

        /// <summary>
        /// Gets the number of packages which is finished operation.
        /// Used only in <see cref="PackageInstallProgressState.Uploading"/>,
        /// <see cref="PackageInstallProgressState.WriteSession"/> and
        /// <see cref="PackageInstallProgressState.PostInstall"/> state.
        /// </summary>
        public int PackageFinished { get; init; }

        /// <summary>
        /// Gets the number of packages required for this operation.
        /// Used only in <see cref="PackageInstallProgressState.Uploading"/>,
        /// <see cref="PackageInstallProgressState.WriteSession"/> and
        /// <see cref="PackageInstallProgressState.PostInstall"/> state.
        /// </summary>
        public int PackageRequired { get; init; }

        /// <summary>
        /// Gets the upload percentage (from <see langword="0"/> to <see langword="100"/>) completed.
        /// Used only in <see cref="PackageInstallProgressState.Uploading"/> state.
        /// </summary>
        public double UploadProgress { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InstallProgressEventArgs"/> class.
        /// Which is used for <see cref="PackageInstallProgressState.Uploading"/> state.
        /// </summary>
        /// <param name="packageUploaded">The number of packages which is finished operation.</param>
        /// <param name="packageRequired">The number of packages required for this operation.</param>
        /// <param name="uploadProgress">Gets the upload percentage (from <see langword="0"/> to <see langword="100"/>) completed.</param>
        public InstallProgressEventArgs(int packageUploaded, int packageRequired, double uploadProgress) : this(PackageInstallProgressState.Uploading)
        {
            PackageFinished = packageUploaded;
            PackageRequired = packageRequired;
            UploadProgress = uploadProgress;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InstallProgressEventArgs"/> class.
        /// Which is used for <see cref="PackageInstallProgressState.Uploading"/>
        /// <see cref="PackageInstallProgressState.WriteSession"/> and
        /// <see cref="PackageInstallProgressState.PostInstall"/> state.
        /// </summary>
        /// <param name="packageFinished">The number of packages which is finished operation.</param>
        /// <param name="packageRequired">The number of packages required for this operation.</param>
        /// <param name="state">The state of the installation.</param>
        public InstallProgressEventArgs(int packageFinished, int packageRequired, PackageInstallProgressState state) : this(state)
        {
            PackageFinished = packageFinished;
            PackageRequired = packageRequired;
        }
    }
}
