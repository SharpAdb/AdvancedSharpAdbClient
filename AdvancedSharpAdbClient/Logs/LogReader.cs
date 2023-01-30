// <copyright file="LogReader.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

using AdvancedSharpAdbClient.Exceptions;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AdvancedSharpAdbClient.Logs
{
    /// <summary>
    /// Processes Android log files in binary format. You usually get the binary format by
    /// running <c>logcat -B</c>.
    /// </summary>
    public class LogReader
    {
        private readonly Stream stream;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogReader"/> class.
        /// </summary>
        /// <param name="stream">A <see cref="Stream"/> that contains the logcat data.</param>
        public LogReader(Stream stream) => this.stream = stream ?? throw new ArgumentNullException(nameof(stream));

        /// <summary>
        /// Reads the next <see cref="LogEntry"/> from the stream.
        /// </summary>
        /// <returns>A new <see cref="LogEntry"/> object.</returns>
        public async Task<LogEntry> ReadEntry(CancellationToken cancellationToken = default)
        {
            //LogEntry value = new();

            // Read the log data in binary format. This format is defined at
            // https://android.googlesource.com/platform/system/core/+/master/include/log/logger.h
            // https://android.googlesource.com/platform/system/core/+/67d7eaf/include/log/logger.h
            ushort? payloadLengthValue = await ReadUInt16Async(cancellationToken).ConfigureAwait(false);
            ushort? headerSizeValue = payloadLengthValue == null ? null : await ReadUInt16Async(cancellationToken).ConfigureAwait(false);
            int? pidValue = headerSizeValue == null ? null : await ReadInt32Async(cancellationToken).ConfigureAwait(false);
            int? tidValue = pidValue == null ? null : await ReadInt32Async(cancellationToken).ConfigureAwait(false);
            int? secValue = tidValue == null ? null : await ReadInt32Async(cancellationToken).ConfigureAwait(false);
            int? nsecValue = secValue == null ? null : await ReadInt32Async(cancellationToken).ConfigureAwait(false);

            if (nsecValue == null)
            {
                return null;
            }

            ushort payloadLength = payloadLengthValue.Value;
            ushort headerSize = headerSizeValue.Value;
            int pid = pidValue.Value;
            int tid = tidValue.Value;
            int sec = secValue.Value;
            //int nsec = nsecValue.Value;

            // If the headerSize is not 0, we have on of the logger_entry_v* objects.
            // In all cases, it appears that they always start with a two uint16's giving the
            // header size and payload length.
            // For both objects, the size should be 0x18
            uint id = 0;
            //uint uid = 0;

            if (headerSize != 0)
            {
                if (headerSize >= 0x18)
                {
                    uint? idValue = await ReadUInt32Async(cancellationToken).ConfigureAwait(false);

                    if (idValue == null)
                    {
                        return null;
                    }

                    id = idValue.Value;
                }

                //if (headerSize >= 0x1c)
                //{
                //    uint? uidValue = await ReadUInt32Async(cancellationToken).ConfigureAwait(false);

                //    if (uidValue == null)
                //    {
                //        return null;
                //    }

                //    uid = uidValue.Value;
                //}

                if (headerSize >= 0x20)
                {
                    // Not sure what this is.
                    _ = await ReadUInt32Async(cancellationToken).ConfigureAwait(false);
                }

                if (headerSize > 0x20)
                {
                    throw new AdbException($"An error occurred while reading data from the ADB stream. Although the header size was expected to be 0x18, a header size of 0x{headerSize:X} was sent by the device");
                }
            }

            byte[] data = await ReadBytesSafeAsync(payloadLength, cancellationToken).ConfigureAwait(false);

            if (data == null)
            {
                return null;
            }

            DateTimeOffset timestamp = Utilities.FromUnixTimeSeconds(sec);

            switch ((LogId)id)
            {
                case LogId.Crash
                    or LogId.Kernel
                    or LogId.Main
                    or LogId.Radio
                    or LogId.System:
                    {
                        // format: <priority:1><tag:N>\0<message:N>\0
                        byte priority = data[0];

                        // Find the first \0 byte in the array. This is the seperator
                        // between the tag and the actual message
                        int tagEnd = 1;

                        while (data[tagEnd] != '\0' && tagEnd < data.Length)
                        {
                            tagEnd++;
                        }

                        // Message should be null termintated, so remove the last entry, too (-2 instead of -1)
                        string tag = Encoding.ASCII.GetString(data, 1, tagEnd - 1);
                        string message = Encoding.ASCII.GetString(data, tagEnd + 1, data.Length - tagEnd - 2);

                        return new AndroidLogEntry()
                        {
                            Data = data,
                            ProcessId = pid,
                            ThreadId = tid,
                            TimeStamp = timestamp,
                            Id = id,
                            Priority = (Priority)priority,
                            Message = message,
                            Tag = tag
                        };
                    }

                case LogId.Events:
                    {
                        // https://android.googlesource.com/platform/system/core.git/+/master/liblog/logprint.c#547
                        EventLogEntry entry = new()
                        {
                            Data = data,
                            ProcessId = pid,
                            ThreadId = tid,
                            TimeStamp = timestamp,
                            Id = id
                        };

                        // Use a stream on the data buffer. This will make sure that,
                        // if anything goes wrong parsing the data, we never go past
                        // the message boundary itself.
                        using (MemoryStream dataStream = new(data))
                        {
                            using BinaryReader reader = new(dataStream);
                            int priority = reader.ReadInt32();

                            while (dataStream.Position < dataStream.Length)
                            {
                                ReadLogEntry(reader, entry.Values);
                            }
                        }

                        return entry;
                    }

                default:
                    return new LogEntry()
                    {
                        Data = data,
                        ProcessId = pid,
                        ThreadId = tid,
                        TimeStamp = timestamp,
                        Id = id
                    };
            }
        }

        private void ReadLogEntry(BinaryReader reader, Collection<object> parent)
        {
            EventLogType type = (EventLogType)reader.ReadByte();

            switch (type)
            {
                case EventLogType.Float:
                    parent.Add(reader.ReadSingle());
                    break;

                case EventLogType.Integer:
                    parent.Add(reader.ReadInt32());
                    break;

                case EventLogType.Long:
                    parent.Add(reader.ReadInt64());
                    break;

                case EventLogType.List:
                    byte listLength = reader.ReadByte();

                    Collection<object> list = new();

                    for (int i = 0; i < listLength; i++)
                    {
                        ReadLogEntry(reader, list);
                    }

                    parent.Add(list);
                    break;

                case EventLogType.String:
                    int stringLength = reader.ReadInt32();
                    byte[] messageData = reader.ReadBytes(stringLength);
                    string message = Encoding.ASCII.GetString(messageData);
                    parent.Add(message);
                    break;
            }
        }

        private async Task<ushort?> ReadUInt16Async(CancellationToken cancellationToken = default)
        {
            byte[] data = await ReadBytesSafeAsync(2, cancellationToken).ConfigureAwait(false);

            return data == null ? null : BitConverter.ToUInt16(data, 0);
        }

        private async Task<uint?> ReadUInt32Async(CancellationToken cancellationToken = default)
        {
            byte[] data = await ReadBytesSafeAsync(4, cancellationToken).ConfigureAwait(false);

            return data == null ? null : BitConverter.ToUInt32(data, 0);
        }

        private async Task<int?> ReadInt32Async(CancellationToken cancellationToken = default)
        {
            byte[] data = await ReadBytesSafeAsync(4, cancellationToken).ConfigureAwait(false);

            return data == null ? null : BitConverter.ToInt32(data, 0);
        }

        private async Task<byte[]> ReadBytesSafeAsync(int count, CancellationToken cancellationToken = default)
        {
            int totalRead = 0;
            int read = 0;

            byte[] data = new byte[count];

            while ((read =
#if !NET35
                await stream.ReadAsync(data, totalRead, count - totalRead, cancellationToken).ConfigureAwait(false)
#else
                await Utilities.Run(() => stream.Read(data, totalRead, count - totalRead)).ConfigureAwait(false)
#endif
                ) > 0)
            {
                totalRead += read;
            }

            return totalRead < count ? null : data;
        }
    }
}
