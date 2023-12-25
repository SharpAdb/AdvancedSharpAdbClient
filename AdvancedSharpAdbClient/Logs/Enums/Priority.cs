// <copyright file="Priority.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

namespace AdvancedSharpAdbClient.Logs
{
    /// <summary>
    /// Represents a log priority.
    /// </summary>
    /// <remarks><seealso href="https://android.googlesource.com/platform/system/logging/+/refs/heads/main/liblog/include/android/log.h#73"/></remarks>
    public enum Priority : byte
    {
        /// <summary>
        /// For internal use only.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// The default priority, for internal use only.
        /// </summary>
        Default,

        /// <summary>
        /// Represents a verbose message.
        /// </summary>
        Verbose,

        /// <summary>
        /// Represents a debug message.
        /// </summary>
        Debug,

        /// <summary>
        /// Represents an informational message.
        /// </summary>
        Info,

        /// <summary>
        /// Represents a warning.
        /// </summary>
        Warn,

        /// <summary>
        /// Represents an error.
        /// </summary>
        Error,

        /// <summary>
        /// Represents an assertion which failed.
        /// </summary>
        Fatal,

        /// <summary>
        /// Represents an assertion which failed.
        /// </summary>
        Assert = Fatal,

        /// <summary>
        /// For internal use only.
        /// </summary>
        Silent
    }
}
