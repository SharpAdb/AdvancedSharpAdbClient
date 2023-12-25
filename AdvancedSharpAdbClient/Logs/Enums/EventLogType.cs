// <copyright file="EventLogType.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

namespace AdvancedSharpAdbClient.Logs
{
    /// <summary>
    /// Represents the different types of values that can be stored in an event log entry.
    /// </summary>
    /// <remarks><seealso href="https://android.googlesource.com/platform/system/logging/+/refs/heads/main/liblog/include/log/log.h#96"/></remarks>
    public enum EventLogType : byte
    {
        /* Special markers for android_log_list_element type */

        /// <summary>
        /// The value declare end of list.
        /// </summary>
        ListStop = (byte)'\n',

        /// <summary>
        /// The value means protocol error.
        /// </summary>
        Unknown = (byte)'?',

        /* must match with declaration in java/android/android/util/EventLog.java */

        /// <summary>
        /// The value is a four-byte signed integer.
        /// </summary>
        Integer = 0,

        /// <summary>
        /// The value is an eight-byte signed integer.
        /// </summary>
        Long,

        /// <summary>
        /// The value is a string.
        /// </summary>
        String,

        /// <summary>
        /// The value is a list of values.
        /// </summary>
        List,

        /// <summary>
        /// The value is a four-byte signed floating number.
        /// </summary>
        Float
    }
}
