// <copyright file="SyncFlag.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;

namespace AdvancedSharpAdbClient.Models
{
    /// <summary>
    /// The sync flags for <see cref="ISyncService"/>.
    /// </summary>
    [Flags]
    public enum SyncFlags : uint
    {
        /// <summary>
        /// No flags set.
        /// </summary>
        None = 0,

        /// <summary>
        /// Use Brotli compression.
        /// </summary>
        Brotli = 1,

        /// <summary>
        /// Use LZ4 compression.
        /// </summary>
        LZ4 = 2,

        /// <summary>
        /// Use Zstd compression.
        /// </summary>
        Zstd = 4,

        /// <summary>
        /// Perform a dry run.
        /// </summary>
        DryRun = 0x8000_0000U
    }
}
