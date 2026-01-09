// <copyright file="SyncCommand.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

namespace AdvancedSharpAdbClient.Models
{
    /// <summary>
    /// Defines a command that can be sent to, or a response that can be received from the sync service.
    /// </summary>
    public enum SyncCommand
    {
        /// <summary>
        /// Stat a file.
        /// </summary>
        STAT = 'S' | ('T' << 8) | ('A' << 16) | ('T' << 24),

        /// <summary>
        /// Stat a file v2.
        /// </summary>
        /// <remarks>Need Android 8 or above.</remarks>
        STA2 = 'S' | ('T' << 8) | ('A' << 16) | ('2' << 24),

        /// <summary>
        /// Stat a list v2.
        /// </summary>
        /// <remarks>Need Android 8 or above.</remarks>
        LST2 = 'L' | ('S' << 8) | ('T' << 16) | ('2' << 24),

        /// <summary>
        /// List the files in a folder.
        /// </summary>
        LIST = 'L' | ('I' << 8) | ('S' << 16) | ('T' << 24),

        /// <summary>
        /// List the files in a folder v2.
        /// </summary>
        /// <remarks>Need Android 11 or above.</remarks>
        LIS2 = 'L' | ('I' << 8) | ('S' << 16) | ('2' << 24),

        /// <summary>
        /// A directory entry.
        /// </summary>
        DENT = 'D' | ('E' << 8) | ('N' << 16) | ('T' << 24),

        /// <summary>
        /// A directory entry v2.
        /// </summary>
        DNT2 = 'D' | ('N' << 8) | ('T' << 16) | ('2' << 24),

        /// <summary>
        /// Send a file to device.
        /// </summary>
        SEND = 'S' | ('E' << 8) | ('N' << 16) | ('D' << 24),

        /// <summary>
        /// Retrieve a file from device v2.
        /// </summary>
        SND2 = 'S' | ('N' << 8) | ('D' << 16) | ('2' << 24),

        /// <summary>
        /// Retrieve a file from device.
        /// </summary>
        RECV = 'R' | ('E' << 8) | ('C' << 16) | ('V' << 24),

        /// <summary>
        /// Retrieve a file from device v2.
        /// </summary>
        RCV2 = 'R' | ('C' << 8) | ('V' << 16) | ('2' << 24),

        /// <summary>
        /// The operation has completed.
        /// </summary>
        DONE = 'D' | ('O' << 8) | ('N' << 16) | ('E' << 24),

        /// <summary>
        /// Marks the start of a data packet.
        /// </summary>
        DATA = 'D' | ('A' << 8) | ('T' << 16) | ('A' << 24),

        /// <summary>
        /// The server has acknowledged the request.
        /// </summary>
        OKAY = 'O' | ('K' << 8) | ('A' << 16) | ('Y' << 24),

        /// <summary>
        /// The operation has failed.
        /// </summary>
        FAIL = 'F' | ('A' << 8) | ('I' << 16) | ('L' << 24),

        /// <summary>
        /// The server has acknowledged the request.
        /// </summary>
        QUIT = 'Q' | ('U' << 8) | ('I' << 16) | ('T' << 24)
    }
}
