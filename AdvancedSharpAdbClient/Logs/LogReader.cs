// <copyright file="LogReader.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace AdvancedSharpAdbClient.Logs
{
    /// <summary>
    /// Processes Android log files in binary format. You usually get the binary format by running <c>logcat -B</c>.
    /// </summary>
    /// <param name="stream">A <see cref="Stream"/> that contains the logcat data.</param>
    public partial class LogReader(Stream stream)
    {
        /// <summary>
        /// The <see cref="Stream"/> that contains the logcat data.
        /// </summary>
        private readonly Stream stream = stream ?? throw new ArgumentNullException(nameof(stream));

        /// <summary>
        /// Reads the next <see cref="LogEntry"/> from the stream.
        /// </summary>
        /// <returns>A new <see cref="LogEntry"/> object.</returns>
        public virtual LogEntry? ReadEntry()
        {
            // Read the log data in binary format. This format is defined at
            // https://android.googlesource.com/platform/system/logging/+/refs/heads/main/liblog/include/log/log_read.h#39
            ushort? payloadLengthValue = ReadUInt16();
            ushort? headerSizeValue = payloadLengthValue == null ? null : ReadUInt16();
            int? pidValue = headerSizeValue == null ? null : ReadInt32();
            uint? tidValue = pidValue == null ? null : ReadUInt32();
            uint? secValue = tidValue == null ? null : ReadUInt32();
            uint? nsecValue = secValue == null ? null : ReadUInt32();

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
                    uint? idValue = ReadUInt32();

                    if (idValue == null)
                    {
                        return null;
                    }

                    uid = idValue.Value;
                    id = (LogId)uid;
                }

                if (headerSize >= 0x1c)
                {
                    uint? uidValue = ReadUInt32();

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
                        _ = ReadUInt32();
                    }
                    else
                    {
                        Debug.WriteLine($"An error occurred while reading data from the ADB stream. Although the header size was expected to be 0x18, a header size of 0x{headerSize:X} was sent by the device");
                        return null;
                    }
                }
            }

            byte[]? data = ReadBytesSafe(payloadLength);

            if (data == null)
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
                    string tag = AdbClient.Encoding.GetString(data, 1, tagEnd - 1);
                    string message = AdbClient.Encoding.GetString(data, tagEnd + 1, data.Length - tagEnd - 2);

                    return new AndroidLogEntry
                    {
                        Data = data,
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
                    // https://android.googlesource.com/platform/system/core.git/+/master/liblog/logprint.c#547
                    EventLogEntry entry = new()
                    {
                        Data = data,
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
                    using (MemoryStream dataStream = new(data))
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
                        Data = data,
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
        }

        /// <summary>
        /// Reads a single log entry from the stream.
        /// </summary>
        protected static void ReadLogEntry(BinaryReader reader, ICollection<object> parent)
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

        /// <summary>
        /// Reads a <see cref="ushort"/> from the stream.
        /// </summary>
        /// <returns>The <see cref="ushort"/> that was read.</returns>
        protected ushort? ReadUInt16()
        {
            byte[]? data = ReadBytesSafe(2);
            return data == null ? null : (ushort)(data[0] | (data[1] << 8));
        }

        /// <summary>
        /// Reads a <see cref="uint"/> from the stream.
        /// </summary>
        /// <returns>The <see cref="uint"/> that was read.</returns>
        protected uint? ReadUInt32()
        {
            byte[]? data = ReadBytesSafe(4);
            return data == null ? null : (uint)(data[0] | (data[1] << 8) | (data[2] << 16) | (data[3] << 24));
        }

        /// <summary>
        /// Reads a <see cref="int"/> from the stream.
        /// </summary>
        /// <returns>The <see cref="int"/> that was read.</returns>
        protected int? ReadInt32()
        {
            byte[]? data = ReadBytesSafe(4);
            return data == null ? null : data[0] | (data[1] << 8) | (data[2] << 16) | (data[3] << 24);
        }

        /// <summary>
        /// Reads bytes from the stream, making sure that the requested number of bytes
        /// </summary>
        /// <param name="count">The number of bytes to read.</param>
        /// <returns>The bytes that were read.</returns>
        protected byte[]? ReadBytesSafe(int count)
        {
            int totalRead = 0;
            byte[] data = new byte[count];

            int read;
#if HAS_BUFFERS
            while ((read = stream.Read(data.AsSpan(totalRead))) > 0)
#else
            while ((read = stream.Read(data, totalRead, count - totalRead)) > 0)
#endif
            {
                totalRead += read;
            }

            return totalRead < count ? null : data;
        }
    }
}
