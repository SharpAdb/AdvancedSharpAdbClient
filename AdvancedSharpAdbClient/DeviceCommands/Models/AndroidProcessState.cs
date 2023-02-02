// <copyright file="AndroidProcessState.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

namespace AdvancedSharpAdbClient.DeviceCommands
{
    /// <summary>
    /// Represents the state of a process running on an Android device.
    /// </summary>
    public enum AndroidProcessState
    {
        /// <summary>
        /// The process state is unknown.
        /// </summary>
        Unknown,

        /// <summary>
        /// Running or runnable (on run queue).
        /// </summary>
        R,

        /// <summary>
        /// Interruptible sleep (waiting for an event to complete).
        /// </summary>
        S,

        /// <summary>
        /// Uninterruptible sleep (usually IO).
        /// </summary>
        D,

        /// <summary>
        /// Defunct ("zombie") process, terminated but not reaped by its parent.
        /// </summary>
        Z,

        /// <summary>
        /// Stopped, either by a job control signal or because it is being traced.
        /// </summary>
        T,

        /// <summary>
        /// paging (not valid since the 2.6.xx kernel).
        /// </summary>
        W,

        /// <summary>
        /// dead (should never be seen).
        /// </summary>
        X,

        /// <summary>
        /// Wakekill.
        /// </summary>
        K,

        /// <summary>
        /// Parked.
        /// </summary>
        P
    }
}
