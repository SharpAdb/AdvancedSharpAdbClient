// <copyright file="EventLogType.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

namespace AdvancedSharpAdbClient.Logs
{
    /// <summary>
    /// Represents the different types of values that can be stored in an event log entry.
    /// </summary>
    public enum EventLogType : byte
    {
        /// <summary>
        /// The value is a four-byte signed integer.
        /// </summary>
        Integer,

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
