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
        STAT = 1,

        /// <summary>
        /// List the files in a folder.
        /// </summary>
        LIST,

        /// <summary>
        /// Send a file to device.
        /// </summary>
        SEND,

        /// <summary>
        /// Retrieve a file from device.
        /// </summary>
        RECV,

        /// <summary>
        /// A directory entry.
        /// </summary>
        DENT,

        /// <summary>
        /// The operation has completed.
        /// </summary>
        DONE,

        /// <summary>
        /// Marks the start of a data packet.
        /// </summary>
        DATA,

        /// <summary>
        /// The server has acknowledged the request.
        /// </summary>
        OKAY,

        /// <summary>
        /// The operation has failed.
        /// </summary>
        FAIL,

        /// <summary>
        /// The server has acknowledged the request.
        /// </summary>
        QUIT,

        /// <summary>
        /// Stat a file v2.
        /// </summary>
        STA2,

        /// <summary>
        /// Stat a list v2.
        /// </summary>
        LST2
    }
}
