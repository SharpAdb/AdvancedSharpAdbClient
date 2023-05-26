// <copyright file="AppStatus.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

namespace AdvancedSharpAdbClient
{
    /// <summary>
    /// The status of an application if it is stopped or running in foreground or background.
    /// </summary>
    public enum AppStatus
    {
        /// <summary>
        /// The application is stopped.
        /// </summary>
        Stopped = 0B00,

        /// <summary>
        /// The application is running in background.
        /// </summary>
        Background = 0B01,

        /// <summary>
        /// The application is running in foreground.
        /// </summary>
        Foreground = 0B11,
    }
}
