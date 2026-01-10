// <copyright file="IFileStatistics.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;

namespace AdvancedSharpAdbClient
{
    /// <summary>
    /// Contains information about a file on the remote device.
    /// </summary>
    public interface IFileStatistics
    {
        /// <summary>
        /// Gets the path of the file.
        /// </summary>
        string Path { get; }

        /// <summary>
        /// Gets the <see cref="UnixFileStatus"/> attributes of the file.
        /// </summary>
        UnixFileStatus FileMode { get; }

        /// <summary>
        /// Gets the total file size, in bytes.
        /// </summary>
        ulong Size { get; }

        /// <summary>
        /// Gets the time of last modification.
        /// </summary>
        DateTimeOffset Time { get; }
    }
}
