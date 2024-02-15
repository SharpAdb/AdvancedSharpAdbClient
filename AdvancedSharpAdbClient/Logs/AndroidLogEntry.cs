// <copyright file="AndroidLogEntry.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Diagnostics;

namespace AdvancedSharpAdbClient.Logs
{
    /// <summary>
    /// Represents a standard Android log entry (an entry in any Android log buffer except the Event buffer).
    /// </summary>
    /// <remarks><seealso href="https://android.googlesource.com/platform/system/logging/+/refs/heads/main/liblog/logprint.cpp"/></remarks>
    [DebuggerDisplay($"{nameof(AndroidLogEntry)} \\{{ {nameof(TimeStamp)} = {{{nameof(TimeStamp)}}}, {nameof(ProcessId)} = {{{nameof(ProcessId)}}}, {nameof(Priority)} = {{{nameof(Priority)}}}, {nameof(Tag)} = {{{nameof(Tag)}}}, {nameof(Message)} = {{{nameof(Message)}}} }}")]
    public class AndroidLogEntry : LogEntry
    {
        /// <summary>
        /// Maps Android log priorities to chars used to represent them in the system log.
        /// </summary>
        private static readonly Dictionary<Priority, char> PriorityFormatters = new(6)
        {
            { Priority.Verbose, 'V' },
            { Priority.Debug, 'D' },
            { Priority.Info, 'I' },
            { Priority.Warn, 'W' },
            { Priority.Error, 'E' },
            { Priority.Fatal, 'F' },
            { Priority.Silent, 'S' }
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="AndroidLogEntry"/> class.
        /// </summary>
        public AndroidLogEntry() { }

        /// <summary>
        /// Gets or sets the priority of the log message.
        /// </summary>
        public Priority Priority { get; set; }

        /// <summary>
        /// Gets or sets the log tag of the message. Used to identify the source of a log message.
        /// It usually identifies the class or activity where the log call occurred.
        /// </summary>
        public string Tag { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the message that has been logged.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <inheritdoc/>
        public override string ToString() =>
            $"{TimeStamp.LocalDateTime:yy-MM-dd HH:mm:ss.fff} {ProcessId,5} {ProcessId,5} {FormatPriority(Priority)} {Tag,-8}: {Message}";

        /// <summary>
        /// Converts a <see cref="Priority"/> value to a char that represents that value in the system log.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>A <see cref="char"/> that represents <paramref name="value"/> in the system log.</returns>
        private static char FormatPriority(Priority value) =>
            PriorityFormatters.TryGetValue(value, out char result) == true ? result : '?';
    }
}
