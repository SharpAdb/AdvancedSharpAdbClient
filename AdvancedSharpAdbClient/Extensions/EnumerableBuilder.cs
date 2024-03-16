#if HAS_BUFFERS
// <copyright file="EnumerableBuilder.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace AdvancedSharpAdbClient
{
    /// <summary>
    /// A collection builder provide for collection expressions.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class EnumerableBuilder
    {
        /// <summary>
        /// Build a <see cref="AdbCommandLineStatus"/> struct.
        /// </summary>
        /// <param name="values">The data that feeds the <see cref="AdbCommandLineStatus"/> struct.</param>
        /// <returns>A new instance of <see cref="AdbCommandLineStatus"/> struct.</returns>
        public static AdbCommandLineStatus AdbCommandLineStatusCreator(ReadOnlySpan<string> values) =>
            AdbCommandLineStatus.GetVersionFromOutput(values);

        /// <summary>
        /// Build a <see cref="ColorData"/> struct.
        /// </summary>
        /// <param name="values">The data that feeds the <see cref="ColorData"/> struct.</param>
        /// <returns>A new instance of <see cref="ColorData"/> struct.</returns>
        public static ColorData ColorDataCreator(ReadOnlySpan<byte> values) =>
            new((uint)(values[0] | (values[1] << 8) | (values[2] << 16) | (values[3] << 24)),
                (uint)(values[4] | (values[5] << 8) | (values[6] << 16) | (values[7] << 24)));

        /// <summary>
        /// Build a <see cref="FramebufferHeader"/> struct.
        /// </summary>
        /// <param name="values">The data that feeds the <see cref="FramebufferHeader"/> struct.</param>
        /// <returns>A new instance of <see cref="FramebufferHeader"/> struct.</returns>
        public static FramebufferHeader FramebufferHeaderCreator(ReadOnlySpan<byte> values) => new(values);

        /// <summary>
        /// Build a <see cref="FileStatistics"/> struct.
        /// </summary>
        /// <param name="values">The data that feeds the <see cref="FileStatistics"/> struct.</param>
        /// <returns>A new instance of <see cref="FileStatistics"/> struct.</returns>
        public static FileStatistics FileStatisticsCreator(ReadOnlySpan<byte> values)
        {
            int index = 0;
            return new FileStatistics
            {
                FileMode = (UnixFileStatus)ReadInt32(values),
                Size = ReadInt32(values),
                Time = DateTimeExtensions.FromUnixTimeSeconds(ReadInt32(values))
            };
            int ReadInt32(in ReadOnlySpan<byte> data) => data[index++] | (data[index++] << 8) | (data[index++] << 16) | (data[index++] << 24);
        }

        /// <summary>
        /// Build a <see cref="UnixFileStatus"/> enum.
        /// </summary>
        /// <param name="values">The data that feeds the <see cref="UnixFileStatus"/> struct.</param>
        /// <returns>A new instance of <see cref="UnixFileStatus"/> struct.</returns>
        public static UnixFileStatus UnixFileStatusCreator(ReadOnlySpan<byte> values) =>
            (UnixFileStatus)(values[0] | (values[1] << 8) | (values[2] << 16) | (values[3] << 24));

        /// <summary>
        /// Build a <see cref="LogEntry"/> class.
        /// </summary>
        /// <param name="values">The data that feeds the <see cref="LogEntry"/> struct.</param>
        /// <returns>A new instance of <see cref="LogEntry"/> struct.</returns>
        public static LogEntry? LogEntryCreator(ReadOnlySpan<byte> values)
        {
            if (values.IsEmpty) { return null; }
            int index = 0;

            // Read the log data in binary format. This format is defined at
            // https://android.googlesource.com/platform/system/logging/+/refs/heads/main/liblog/include/log/log_read.h#39
            ushort? payloadLengthValue = ReadUInt16(values);
            ushort? headerSizeValue = payloadLengthValue == null ? null : ReadUInt16(values);
            int? pidValue = headerSizeValue == null ? null : ReadInt32(values);
            uint? tidValue = pidValue == null ? null : ReadUInt32(values);
            uint? secValue = tidValue == null ? null : ReadUInt32(values);
            uint? nsecValue = secValue == null ? null : ReadUInt32(values);

            if (nsecValue == null)
            {
                return null;
            }

            ushort payloadLength = payloadLengthValue!.Value;
            ushort headerSize = headerSizeValue!.Value;
            int pid = pidValue!.Value;
            uint tid = tidValue!.Value;
            uint sec = secValue!.Value;
            uint nsec = nsecValue.Value;

            // If the headerSize is not 0, we have on of the logger_entry_v* objects.
            // In all cases, it appears that they always start with a two uint16's giving the
            // header size and payload length.
            // For both objects, the size should be 0x18
            LogId id = 0;
            uint uid = 0;

            if (headerSize != 0)
            {
                if (headerSize >= 0x18)
                {
                    uint? idValue = ReadUInt32(values);

                    if (idValue == null)
                    {
                        return null;
                    }

                    uid = idValue.Value;
                    id = (LogId)uid;
                }

                if (headerSize >= 0x1c)
                {
                    uint? uidValue = ReadUInt32(values);

                    if (uidValue == null)
                    {
                        return null;
                    }

                    uid = uidValue.Value;
                }

                if (headerSize > 0x20)
                {
                    if (headerSize == 0x20)
                    {
                        // Not sure what this is.
                        _ = ReadUInt32(values);
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException(nameof(values), $"An error occurred while reading data from the ADB stream. Although the header size was expected to be 0x18, a header size of 0x{headerSize:X} was sent by the device");
                    }
                }
            }

            ReadOnlySpan<byte> data = ReadBytesSafe(values, payloadLength);

            if (data.IsEmpty)
            {
                return null;
            }

            DateTimeOffset timestamp = DateTimeExtensions.FromUnixTimeSeconds(sec);

            switch (id)
            {
                case >= LogId.Min and <= LogId.Max and not LogId.Events:
                    // format: <priority:1><tag:N>\0<message:N>\0
                    byte priority = data[0];

                    // Find the first \0 byte in the array. This is the separator
                    // between the tag and the actual message
                    int tagEnd = 1;

                    while (data[tagEnd] != '\0' && tagEnd < data.Length)
                    {
                        tagEnd++;
                    }

                    // Message should be null terminated, so remove the last entry, too (-2 instead of -1)
                    string tag = AdbClient.Encoding.GetString(data[1..tagEnd]);
                    string message = AdbClient.Encoding.GetString(data.Slice(tagEnd + 1, data.Length - tagEnd - 2));

                    return new AndroidLogEntry
                    {
                        Data = data.ToArray(),
                        PayloadLength = payloadLength,
                        HeaderSize = headerSize,
                        ProcessId = pid,
                        ThreadId = tid,
                        TimeStamp = timestamp,
                        NanoSeconds = nsec,
                        Id = id,
                        Uid = uid,
                        Priority = (Priority)priority,
                        Message = message,
                        Tag = tag
                    };

                case LogId.Events:
                    byte[] dataArray = data.ToArray();

                    // https://android.googlesource.com/platform/system/core.git/+/master/liblog/logprint.c#547
                    EventLogEntry entry = new()
                    {
                        Data = dataArray,
                        PayloadLength = payloadLength,
                        HeaderSize = headerSize,
                        ProcessId = pid,
                        ThreadId = tid,
                        TimeStamp = timestamp,
                        NanoSeconds = nsec,
                        Id = id,
                        Uid = uid
                    };

                    // Use a stream on the data buffer. This will make sure that,
                    // if anything goes wrong parsing the data, we never go past
                    // the message boundary itself.
                    using (MemoryStream dataStream = new(dataArray))
                    {
                        using BinaryReader reader = new(dataStream);
                        _ = reader.ReadInt32();

                        while (dataStream.Position < dataStream.Length)
                        {
                            ReadLogEntry(reader, entry.Values);
                        }
                    }

                    return entry;

                default:
                    return new LogEntry
                    {
                        Data = data.ToArray(),
                        PayloadLength = payloadLength,
                        HeaderSize = headerSize,
                        ProcessId = pid,
                        ThreadId = tid,
                        TimeStamp = timestamp,
                        NanoSeconds = nsec,
                        Id = id,
                        Uid = uid
                    };
            }

            static void ReadLogEntry(BinaryReader reader, ICollection<object> parent)
            {
                EventLogType type = (EventLogType)reader.ReadByte();

                switch (type)
                {
                    case EventLogType.Integer:
                        parent.Add(reader.ReadInt32());
                        break;

                    case EventLogType.Long:
                        parent.Add(reader.ReadInt64());
                        break;

                    case EventLogType.String:
                        int stringLength = reader.ReadInt32();
                        byte[] messageData = reader.ReadBytes(stringLength);
                        string message = AdbClient.Encoding.GetString(messageData);
                        parent.Add(message);
                        break;

                    case EventLogType.List:
                        byte listLength = reader.ReadByte();
                        List<object> list = new(listLength);
                        for (int i = 0; i < listLength; i++)
                        {
                            ReadLogEntry(reader, list);
                        }
                        parent.Add(list);
                        break;

                    case EventLogType.Float:
                        parent.Add(reader.ReadSingle());
                        break;
                }
            }

            ushort? ReadUInt16(in ReadOnlySpan<byte> bytes)
            {
                ReadOnlySpan<byte> data = ReadBytesSafe(bytes, 2);
                return data == null ? null : (ushort)(data[0] | (data[1] << 8));
            }

            uint? ReadUInt32(in ReadOnlySpan<byte> bytes)
            {
                ReadOnlySpan<byte> data = ReadBytesSafe(bytes, 4);
                return data == null ? null : (uint)(data[0] | (data[1] << 8) | (data[2] << 16) | (data[3] << 24));
            }

            int? ReadInt32(in ReadOnlySpan<byte> bytes)
            {
                ReadOnlySpan<byte> data = ReadBytesSafe(bytes, 4);
                return data.Length != 4 ? null : data[0] | (data[1] << 8) | (data[2] << 16) | (data[3] << 24);
            }

            ReadOnlySpan<byte> ReadBytesSafe(in ReadOnlySpan<byte> bytes, int count)
            {
                if (bytes.Length < index + count) { return null; }
                ReadOnlySpan<byte> data = bytes.Slice(index, count);
                index += count;
                return data;
            }
        }
    }
}
#endif