// <copyright file="EventLogEntry.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Diagnostics;

namespace AdvancedSharpAdbClient.Logs
{
    /// <summary>
    /// Represents an entry in event buffer of the the Android log.
    /// </summary>
    /// <remarks><seealso href="https://android.googlesource.com/platform/system/core/+/master/include/log/log.h#482"/></remarks>
    [DebuggerDisplay($"{nameof(AndroidLogEntry)} \\{{ {nameof(TimeStamp)} = {{{nameof(TimeStamp)}}}, {nameof(ProcessId)} = {{{nameof(ProcessId)}}}, {nameof(Tag)} = {{{nameof(Tag)}}}, {nameof(Values)} = {{{nameof(Values)}}} }}")]
    public class EventLogEntry : LogEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventLogEntry"/> class.
        /// </summary>
        public EventLogEntry() { }

        /// <summary>
        /// Gets or sets the 4 bytes integer key from <c>"/system/etc/event-log-tags"</c> file.
        /// </summary>
        public int Tag { get; set; }

        /// <summary>
        /// Gets or sets the values of this event log entry.
        /// </summary>
        /// <remarks>Values can be of type <see cref="int"/>, <see cref="long"/>, <see cref="string"/>, <see cref="List{Object}"/> or <see cref="float"/>.</remarks>
        public List<object> Values { get; set; } = [];

        /// <inheritdoc/>
        public override string ToString() =>
            $"{TimeStamp.LocalDateTime:yy-MM-dd HH:mm:ss.fff} {ProcessId,5} {ProcessId,5} {Tag,-8}: {StringExtensions.Join(", ", Values)}";
    }
}
