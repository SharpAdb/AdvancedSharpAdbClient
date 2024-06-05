#if HAS_TASK
// <copyright file="LogReader.Async.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace AdvancedSharpAdbClient.Logs
{
    public partial class LogReader
    {
        /// <summary>
        /// Asynchronously reads the next <see cref="LogEntry"/> from the stream.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task{LogEntry}"/> which returns a new <see cref="LogEntry"/> object.</returns>
        public async Task<LogEntry?> ReadEntryAsync(CancellationToken cancellationToken = default)
        {
            // Read the log data in binary format. This format is defined at
            // https://android.googlesource.com/platform/system/logging/+/refs/heads/main/liblog/include/log/log_read.h#39
            ushort? payloadLengthValue = await ReadUInt16Async(cancellationToken).ConfigureAwait(false);
            ushort? headerSizeValue = payloadLengthValue == null ? null : await ReadUInt16Async(cancellationToken).ConfigureAwait(false);
            int? pidValue = headerSizeValue == null ? null : await ReadInt32Async(cancellationToken).ConfigureAwait(false);
            uint? tidValue = pidValue == null ? null : await ReadUInt32Async(cancellationToken).ConfigureAwait(false);
            uint? secValue = tidValue == null ? null : await ReadUInt32Async(cancellationToken).ConfigureAwait(false);
            uint? nsecValue = secValue == null ? null : await ReadUInt32Async(cancellationToken).ConfigureAwait(false);

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
                    uint? idValue = await ReadUInt32Async(cancellationToken).ConfigureAwait(false);

                    if (idValue == null)
                    {
                        return null;
                    }

                    uid = idValue.Value;
                    id = (LogId)uid;
                }

                if (headerSize >= 0x1c)
                {
                    uint? uidValue = await ReadUInt32Async(cancellationToken).ConfigureAwait(false);

                    if (uidValue == null)
                    {
                        return null;
                    }

                    uid = uidValue.Value;
                }

                if (headerSize >= 0x20)
                {
                    if (headerSize == 0x20)
                    {
                        // Not sure what this is.
                        _ = await ReadUInt32Async(cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        Debug.WriteLine($"An error occurred while reading data from the ADB stream. Although the header size was expected to be 0x18, a header size of 0x{headerSize:X} was sent by the device");
                        return null;
                    }
                }
            }

            byte[]? data = await ReadBytesSafeAsync(payloadLength, cancellationToken).ConfigureAwait(false);

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
#if COMP_NETSTANDARD2_1
                    await
#endif
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
        /// Asynchronously reads a <see cref="ushort"/> from the stream.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task{String}"/> which returns the <see cref="ushort"/> value.</returns>
        private async Task<ushort?> ReadUInt16Async(CancellationToken cancellationToken = default)
        {
            byte[]? data = await ReadBytesSafeAsync(2, cancellationToken).ConfigureAwait(false);
            return data == null ? null : (ushort)(data[0] | (data[1] << 8));
        }

        /// <summary>
        /// Asynchronously reads a <see cref="uint"/> from the stream.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task{UInt32}"/> which returns the <see cref="uint"/> value.</returns>
        private async Task<uint?> ReadUInt32Async(CancellationToken cancellationToken = default)
        {
            byte[]? data = await ReadBytesSafeAsync(4, cancellationToken).ConfigureAwait(false);
            return data == null ? null : (uint)(data[0] | (data[1] << 8) | (data[2] << 16) | (data[3] << 24));
        }

        /// <summary>
        /// Asynchronously reads a <see cref="int"/> from the stream.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task{Int32}"/> which returns the <see cref="int"/> value.</returns>
        private async Task<int?> ReadInt32Async(CancellationToken cancellationToken = default)
        {
            byte[]? data = await ReadBytesSafeAsync(4, cancellationToken).ConfigureAwait(false);
            return data == null ? null : data[0] | (data[1] << 8) | (data[2] << 16) | (data[3] << 24);
        }

        /// <summary>
        /// Asynchronously bytes from the stream, making sure that the requested number of bytes
        /// </summary>
        /// <param name="count">The number of bytes to read.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task{Array}"/> which returns the <see cref="byte"/> array.</returns>
        private async Task<byte[]?> ReadBytesSafeAsync(int count, CancellationToken cancellationToken = default)
        {
            int totalRead = 0;
            byte[] data = new byte[count];

            int read;
#if HAS_BUFFERS
            while ((read = await stream.ReadAsync(data.AsMemory(totalRead, count - totalRead), cancellationToken).ConfigureAwait(false)) > 0)
#else
            while ((read = await stream.ReadAsync(data, totalRead, count - totalRead, cancellationToken).ConfigureAwait(false)) > 0)
#endif
            {
                totalRead += read;
            }

            return totalRead < count ? null : data;
        }
    }
}
#endif