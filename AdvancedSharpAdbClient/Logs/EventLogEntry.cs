// <copyright file="EventLogEntry.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;

namespace AdvancedSharpAdbClient.Logs
{
    /// <summary>
    /// Represents an entry in event buffer of the the Android log.
    /// </summary>
    /// <remarks><seealso href="https://android.googlesource.com/platform/system/core/+/master/include/log/log.h#482"/></remarks>
    public class EventLogEntry : LogEntry
    {
        /// <summary>
        /// Gets or sets the 4 bytes integer key from <c>"/system/etc/event-log-tags"</c> file.
        /// </summary>
        public int Tag { get; set; }

        /// <summary>
        /// Gets or sets the values of this event log entry.
        /// </summary>
        public Collection<object> Values { get; set; } = new Collection<object>();
    }
}
