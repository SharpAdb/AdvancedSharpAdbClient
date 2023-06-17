﻿namespace AdvancedSharpAdbClient.DeviceCommands
{
    /// <summary>
    /// Represents the state of the installation.
    /// </summary>
    public enum PackageInstallProgressState
    {
        /// <summary>
        /// Uploading packages to the device.
        /// </summary>
        Uploading,

        /// <summary>
        /// Create the session for the installation.
        /// </summary>
        CreateSession,

        /// <summary>
        /// Write the package to link with session.
        /// </summary>
        WriteSession,

        /// <summary>
        /// The install is in progress.
        /// </summary>
        Installing,

        /// <summary>
        /// The installation has completed and cleanup actions are in progress.
        /// </summary>
        PostInstall,

        /// <summary>
        /// The operation has completed.
        /// </summary>
        Finished,
    }
}