// <copyright file="LogEntry.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace AdvancedSharpAdbClient.Logs
{
    /// <summary>
    /// The user space structure for version 1 of the logger_entry ABI.
    /// This structure is returned to user space by the kernel logger
    /// driver unless an upgrade to a newer ABI version is requested.
    /// </summary>
    /// <remarks><seealso href="https://android.googlesource.com/platform/system/core/+/master/include/log/logger.h"/></remarks>
#if HAS_BUFFERS
    [CollectionBuilder(typeof(EnumerableBuilder), nameof(EnumerableBuilder.LogEntryCreator))]
#endif
    [DebuggerDisplay($"{nameof(AndroidLogEntry)} \\{{ {nameof(PayloadLength)} = {{{nameof(PayloadLength)}}}, {nameof(HeaderSize)} = {{{nameof(HeaderSize)}}}, {nameof(ProcessId)} = {{{nameof(ProcessId)}}}, {nameof(ThreadId)} = {{{nameof(ThreadId)}}}, {nameof(TimeStamp)} = {{{nameof(TimeStamp)}}}, {nameof(NanoSeconds)} = {{{nameof(NanoSeconds)}}}, {nameof(Id)} = {{{nameof(Id)}}}, {nameof(Uid)} = {{{nameof(Uid)}}}, {nameof(Data)} = {{{nameof(Data)}}} }}")]
    public class LogEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LogEntry"/> class.
        /// </summary>
        public LogEntry() { }

        /// <summary>
        /// Gets or sets the length of the payload.
        /// </summary>
        public ushort PayloadLength { get; set; }

        /// <summary>
        /// Gets or sets the size of the header.
        /// </summary>
        public ushort HeaderSize { get; set; }

        /// <summary>
        /// Gets or sets the process ID of the code that generated the log message.
        /// </summary>
        public int ProcessId { get; set; }

        /// <summary>
        /// Gets or sets the thread ID of the code that generated the log message.
        /// </summary>
        public uint ThreadId { get; set; }

        /// <summary>
        /// Gets or sets the date and time at which the message was logged.
        /// </summary>
        public DateTimeOffset TimeStamp { get; set; }

        /// <summary>
        /// Gets or sets the nanoseconds at which the message was logged.
        /// </summary>
        public uint NanoSeconds { get; set; }

        /// <summary>
        /// Gets or sets the log id (v3) of the payload effective UID of logger (v2);
        /// this value is not available for v1 entries.
        /// </summary>
        public LogId Id { get; set; }

        /// <summary>
        /// Gets or sets the payload effective UID of logger;
        /// this value is not available for v1 entries.
        /// </summary>
        public uint Uid { get; set; }

        /// <summary>
        /// Gets or sets the entry's payload.
        /// </summary>
        public byte[] Data { get; set; } = [];

        /// <inheritdoc/>
        public override string ToString() =>
            $"{TimeStamp.LocalDateTime:yy-MM-dd HH:mm:ss.fff} {ProcessId,5} {ProcessId,5}";

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="LogEntry"/>.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the <see cref="LogEntry"/>.</returns>
        public IEnumerator<byte> GetEnumerator()
        {
            yield return (byte)PayloadLength;
            yield return (byte)(PayloadLength >> 8);

            yield return (byte)HeaderSize;
            yield return (byte)(HeaderSize >> 8);

            yield return (byte)ProcessId;
            yield return (byte)(ProcessId >> 8);
            yield return (byte)(ProcessId >> 16);
            yield return (byte)(ProcessId >> 24);

            yield return (byte)ThreadId;
            yield return (byte)(ThreadId >> 8);
            yield return (byte)(ThreadId >> 16);
            yield return (byte)(ThreadId >> 24);

            long time = TimeStamp.ToUnixTimeSeconds();
            yield return (byte)time;
            yield return (byte)(time >> 8);
            yield return (byte)(time >> 16);
            yield return (byte)(time >> 24);

            yield return (byte)NanoSeconds;
            yield return (byte)(NanoSeconds >> 8);
            yield return (byte)(NanoSeconds >> 16);
            yield return (byte)(NanoSeconds >> 24);

            if (HeaderSize >= 0x18)
            {
                yield return (byte)Id;
                yield return (byte)((uint)Id >> 8);
                yield return (byte)((uint)Id >> 16);
                yield return (byte)((uint)Id >> 24);
            }

            if (HeaderSize >= 0x1c)
            {
                yield return (byte)Uid;
                yield return (byte)(Uid >> 8);
                yield return (byte)(Uid >> 16);
                yield return (byte)(Uid >> 24);
            }

            if (HeaderSize == 0x20)
            {
                yield return 0;
                yield return 0;
                yield return 0;
                yield return 0;
            }

            foreach (byte data in Data)
            {
                yield return data;
            }
        }
    }
}
