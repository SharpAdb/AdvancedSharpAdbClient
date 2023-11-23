// <copyright file="AppStatus.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;

namespace AdvancedSharpAdbClient.Models
{
    /// <summary>
    /// The status of an application if it is stopped or running in foreground or background.
    /// </summary>
    [Flags]
    public enum AppStatus
    {
        /// <summary>
        /// The application is stopped.
        /// </summary>
        Stopped = 0b00,

        /// <summary>
        /// The application is running in background.
        /// </summary>
        Background = 0b01,

        /// <summary>
        /// The application is running in foreground.
        /// </summary>
        Foreground = 0b11,
    }
}
