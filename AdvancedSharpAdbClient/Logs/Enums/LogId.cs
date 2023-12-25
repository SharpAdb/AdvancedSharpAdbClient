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
        Main = 0,

        /// <summary>
        /// The minimum log id.
        /// </summary>
        Min = Main,

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
        /// The Android statistics log buffer.
        /// </summary>
        Stats,

        /// <summary>
        /// The Android security log buffer.
        /// </summary>
        Security,

        /// <summary>
        /// The Android kernel log buffer.
        /// </summary>
        Kernel,

        /// <summary>
        /// The maximum log id.
        /// </summary>
        Max = Kernel,

        /// <summary>
        /// Let the logging function choose the best log target.
        /// </summary>
        Default = 0x7FFFFFFF,

        /// <summary>
        /// All Android log buffers.
        /// </summary>
        All = 0xFFFFFFFF
    }
}
