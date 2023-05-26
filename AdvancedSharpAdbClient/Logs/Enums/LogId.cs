// <copyright file="LogId.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

namespace AdvancedSharpAdbClient.Logs
{
    /// <summary>
    /// Identifies the various Android log buffers.
    /// </summary>
    /// <remarks><seealso href="https://android.googlesource.com/platform/system/core/+/master/include/log/log.h#596"/></remarks>
    public enum LogId : uint
    {
        /// <summary>
        /// The main log buffer
        /// </summary>
        Main,

        /// <summary>
        /// The buffer that contains radio/telephony related messages.
        /// </summary>
        Radio,

        /// <summary>
        /// The buffer containing events-related messages.
        /// </summary>
        Events,

        /// <summary>
        /// The Android system log buffer.
        /// </summary>
        System,

        /// <summary>
        /// The Android crash log buffer.
        /// </summary>
        Crash,

        /// <summary>
        /// The Android kernel log buffer.
        /// </summary>
        Kernel
    }
}
