// <copyright file="SyncService.EventArgs.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;

namespace AdvancedSharpAdbClient.Models
{
    /// <summary>
    /// Provides data for the <see cref="ISyncService.SyncProgressChanged"/> event.
    /// </summary>
    public class SyncProgressChangedEventArgs(long current, long total) : EventArgs
    {
        /// <summary>
        /// Gets the number of bytes sync to the local computer.
        /// </summary>
        /// <value>An <see cref="long"/> representing the number of sync bytes.</value>
        public long ReceivedBytesSize { get; } = current;

        /// <summary>
        /// Gets the total number of bytes for the sync operation.
        /// </summary>
        /// <value>An <see cref="long"/> representing the total size of the download, in bytes.</value>
        public long TotalBytesToReceive { get; } = total;

        /// <summary>
        /// Gets the number of progress percentage (from <see langword="0"/> to <see langword="100"/>) for the sync operation.
        /// </summary>
        public double ProgressPercentage => TotalBytesToReceive != 0L ? ReceivedBytesSize * 100.0 / TotalBytesToReceive : 0.0;
    }
}
