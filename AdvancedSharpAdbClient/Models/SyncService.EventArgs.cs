// <copyright file="SyncService.EventArgs.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Diagnostics;

namespace AdvancedSharpAdbClient.Models
{
    /// <summary>
    /// Provides data for the <see cref="ISyncService"/> interface.
    /// </summary>
    [DebuggerDisplay($"{{{nameof(GetType)}().{nameof(Type.ToString)}(),nq}} \\{{ {nameof(ReceivedBytesSize)} = {{{nameof(ReceivedBytesSize)}}}, {nameof(TotalBytesToReceive)} = {{{nameof(TotalBytesToReceive)}}}, {nameof(ProgressPercentage)} = {{{nameof(ProgressPercentage)}}} }}")]
    public sealed class SyncProgressChangedEventArgs(ulong current, ulong total) : EventArgs
    {
        /// <summary>
        /// Gets the number of bytes sync to the local computer.
        /// </summary>
        /// <value>An <see cref="ulong"/> representing the number of sync bytes.</value>
        public ulong ReceivedBytesSize => current;

        /// <summary>
        /// Gets the total number of bytes for the sync operation.
        /// </summary>
        /// <value>An <see cref="ulong"/> representing the total size of the download, in bytes.</value>
        public ulong TotalBytesToReceive => total;

        /// <summary>
        /// Gets the number of progress percentage (from <see langword="0"/> to <see langword="100"/>) for the sync operation.
        /// </summary>
        public double ProgressPercentage => TotalBytesToReceive == 0 ? 0 : ReceivedBytesSize * 100d / TotalBytesToReceive;

        /// <inheritdoc/>
        public override string ToString() => $"Sync Progress: {ReceivedBytesSize}/{TotalBytesToReceive} ({ProgressPercentage}%)";
    }
}
