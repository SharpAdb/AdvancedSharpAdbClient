﻿// <copyright file="SyncService.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

using AdvancedSharpAdbClient.Exceptions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;

namespace AdvancedSharpAdbClient
{
    /// <summary>
    /// <para>
    ///     Provides access to the sync service running on the Android device. Allows you to
    ///     list, download and upload files on the device.
    /// </para>
    /// </summary>
    /// <example>
    /// <para>
    ///     To send files to or receive files from your Android device, you can use the following code:
    /// </para>
    /// <code>
    /// void DownloadFile()
    /// {
    ///     var device = AdbClient.Instance.GetDevices().First();
    ///
    ///     using (SyncService service = new SyncService(new AdbSocket(), device))
    ///     using (Stream stream = File.OpenWrite(@"C:\MyFile.txt"))
    ///     {
    ///         service.Pull("/data/MyFile.txt", stream, null, CancellationToken.None);
    ///     }
    /// }
    ///
    /// void UploadFile()
    /// {
    ///     var device = AdbClient.Instance.GetDevices().First();
    ///
    ///     using (SyncService service = new SyncService(new AdbSocket(), device))
    ///     using (Stream stream = File.OpenRead(@"C:\MyFile.txt"))
    ///     {
    ///         service.Push(stream, "/data/MyFile.txt", null, CancellationToken.None);
    ///     }
    /// }
    /// </code>
    /// </example>
    public class SyncService : ISyncService, IDisposable
    {
        /// <summary>
        /// The maximum length of a path on the remote device.
        /// </summary>
        private const int MaxPathLength = 1024;

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncService"/> class.
        /// </summary>
        /// <param name="client">A connection to an adb server.</param>
        /// <param name="device">The device on which to interact with the files.</param>
        public SyncService(IAdbClient client, DeviceData device) : this(Factories.AdbSocketFactory(client.EndPoint), device)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncService"/> class.
        /// </summary>
        /// <param name="socket">A <see cref="IAdbSocket"/> that enables to connection with the adb server.</param>
        /// <param name="device">The device on which to interact with the files.</param>
        public SyncService(IAdbSocket socket, DeviceData device)
        {
            Socket = socket;
            Device = device;

            Open();
        }

        /// <summary>
        /// Gets or sets the maximum size of data to transfer between the device and the PC in one block.
        /// </summary>
        public int MaxBufferSize { get; set; } = 64 * 1024;

        /// <summary>
        /// Gets the device on which the file operations are being executed.
        /// </summary>
        public DeviceData Device { get; private set; }

        /// <summary>
        /// Gets the <see cref="IAdbSocket"/> that enables the connection with the adb server.
        /// </summary>
        public IAdbSocket Socket { get; private set; }

        /// <inheritdoc/>
        public bool IsOpen => Socket != null && Socket.Connected;

        /// <inheritdoc/>
        public void Open()
        {
            // target a specific device
            Socket.SetDevice(Device);

            Socket.SendAdbRequest("sync:");
            _ = Socket.ReadAdbResponse();
        }

        /// <summary>
        /// Reopen this connection.
        /// </summary>
        /// <param name="socket">A <see cref="IAdbSocket"/> that enables to connection with the adb server.</param>
        public void Reopen(IAdbSocket socket)
        {
            if (Socket != null)
            {
                Socket.Dispose();
                Socket = null;
            }
            Socket = socket;
            Open();
        }

        /// <summary>
        /// Reopen this connection.
        /// </summary>
        /// <param name="client">A connection to an adb server.</param>
        public void Reopen(IAdbClient client) => Reopen(Factories.AdbSocketFactory(client.EndPoint));

        /// <inheritdoc/>
        public void Push(Stream stream, string remotePath, int permissions, DateTimeOffset timestamp, IProgress<int> progress, CancellationToken cancellationToken)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (remotePath == null)
            {
                throw new ArgumentNullException(nameof(remotePath));
            }

            if (remotePath.Length > MaxPathLength)
            {
                throw new ArgumentOutOfRangeException(nameof(remotePath), $"The remote path {remotePath} exceeds the maximum path size {MaxPathLength}");
            }

            Socket.SendSyncRequest(SyncCommand.SEND, remotePath, permissions);

            // create the buffer used to read.
            // we read max SYNC_DATA_MAX.
            byte[] buffer = new byte[MaxBufferSize];

            // We need 4 bytes of the buffer to send the 'DATA' command,
            // and an additional X bytes to inform how much data we are
            // sending.
            byte[] dataBytes = SyncCommandConverter.GetBytes(SyncCommand.DATA);
            byte[] lengthBytes = BitConverter.GetBytes(MaxBufferSize);
            int headerSize = dataBytes.Length + lengthBytes.Length;
            int reservedHeaderSize = headerSize;
            int maxDataSize = MaxBufferSize - reservedHeaderSize;
            lengthBytes = BitConverter.GetBytes(maxDataSize);

            // Try to get the total amount of bytes to transfer. This is not always possible, for example,
            // for forward-only streams.
            long totalBytesToProcess = stream.CanSeek ? stream.Length : 0;
            long totalBytesRead = 0;

            // look while there is something to read
            while (true)
            {
                // check if we're canceled
                cancellationToken.ThrowIfCancellationRequested();

                // read up to SYNC_DATA_MAX
                int read = stream.Read(buffer, headerSize, maxDataSize);
                totalBytesRead += read;

                if (read == 0)
                {
                    // we reached the end of the file
                    break;
                }
                else if (read != maxDataSize)
                {
                    // At the end of the line, so we need to recalculate the length of the header
                    lengthBytes = BitConverter.GetBytes(read);
                    headerSize = dataBytes.Length + lengthBytes.Length;
                }

                int startPosition = reservedHeaderSize - headerSize;

                Buffer.BlockCopy(dataBytes, 0, buffer, startPosition, dataBytes.Length);
                Buffer.BlockCopy(lengthBytes, 0, buffer, startPosition + dataBytes.Length, lengthBytes.Length);

                // now send the data to the device
                Socket.Send(buffer, startPosition, read + dataBytes.Length + lengthBytes.Length);

                // Let the caller know about our progress, if requested
                if (progress != null && totalBytesToProcess != 0)
                {
                    progress.Report((int)(100.0 * totalBytesRead / totalBytesToProcess));
                }
            }

            // create the DONE message
            int time = (int)timestamp.ToUnixTimeSeconds();
            Socket.SendSyncRequest(SyncCommand.DONE, time);

            // read the result, in a byte array containing 2 ints
            // (id, size)
            SyncCommand result = Socket.ReadSyncResponse();

            if (result == SyncCommand.FAIL)
            {
                string? message = Socket.ReadSyncString();

                throw new AdbException(message);
            }
            else if (result != SyncCommand.OKAY)
            {
                throw new AdbException($"The server sent an invali repsonse {result}");
            }
        }

        /// <inheritdoc/>
        public void Pull(string remoteFilepath, Stream stream, IProgress<int> progress, CancellationToken cancellationToken)
        {
            if (remoteFilepath == null)
            {
                throw new ArgumentNullException(nameof(remoteFilepath));
            }

            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            // Get file information, including the file size, used to calculate the total amount of bytes to receive.
            FileStatistics? stat = Stat(remoteFilepath);
            long totalBytesToProcess = stat.Size;
            long totalBytesRead = 0;

            byte[] buffer = new byte[MaxBufferSize];

            Socket.SendSyncRequest(SyncCommand.RECV, remoteFilepath);

            while (true)
            {
                SyncCommand response = Socket.ReadSyncResponse();
                cancellationToken.ThrowIfCancellationRequested();

                if (response == SyncCommand.DONE)
                {
                    break;
                }
                else if (response == SyncCommand.FAIL)
                {
                    string? message = Socket.ReadSyncString();
                    throw new AdbException($"Failed to pull '{remoteFilepath}'. {message}");
                }
                else if (response != SyncCommand.DATA)
                {
                    throw new AdbException($"The server sent an invalid response {response}");
                }

                // The first 4 bytes contain the length of the data packet
                byte[]? reply = new byte[4];
                _ = Socket.Read(reply);

                if (!BitConverter.IsLittleEndian)
                {
                    Array.Reverse(reply);
                }

                int size = BitConverter.ToInt32(reply, 0);

                if (size > MaxBufferSize)
                {
                    throw new AdbException($"The adb server is sending {size} bytes of data, which exceeds the maximum chunk size {MaxBufferSize}");
                }

                // now read the length we received
                _ = Socket.Read(buffer, size);
                stream.Write(buffer, 0, size);
                totalBytesRead += size;

                // Let the caller know about our progress, if requested
                if (progress != null && totalBytesToProcess != 0)
                {
                    progress.Report((int)(100.0 * totalBytesRead / totalBytesToProcess));
                }
            }
        }

        /// <inheritdoc/>
        public FileStatistics Stat(string remotePath)
        {
            // create the stat request message.
            Socket.SendSyncRequest(SyncCommand.STAT, remotePath);

            if (Socket.ReadSyncResponse() != SyncCommand.STAT)
            {
                throw new AdbException($"The server returned an invalid sync response.");
            }

            // read the result, in a byte array containing 3 ints
            // (mode, size, time)
            FileStatistics value = new FileStatistics();
            value.Path = remotePath;

            ReadStatistics(value);

            return value;
        }

        /// <inheritdoc/>
        public IEnumerable<FileStatistics> GetDirectoryListing(string remotePath)
        {
            Collection<FileStatistics> value = new Collection<FileStatistics>();

            // create the stat request message.
            Socket.SendSyncRequest(SyncCommand.LIST, remotePath);

            while (true)
            {
                SyncCommand response = Socket.ReadSyncResponse();

                if (response == SyncCommand.DONE)
                {
                    break;
                }
                else if (response != SyncCommand.DENT)
                {
                    throw new AdbException($"The server returned an invalid sync response.");
                }

                FileStatistics entry = new FileStatistics();
                ReadStatistics(entry);
                entry.Path = Socket.ReadSyncString();

                value.Add(entry);
            }

            return value;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (Socket != null)
            {
                Socket.Dispose();
                Socket = null;
            }
        }

        private void ReadStatistics(FileStatistics value)
        {
            byte[] statResult = new byte[12];
            _ = Socket.Read(statResult);

            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(statResult, 0, 4);
                Array.Reverse(statResult, 4, 4);
                Array.Reverse(statResult, 8, 4);
            }

            value.FileMode = (UnixFileMode)BitConverter.ToInt32(statResult, 0);
            value.Size = BitConverter.ToInt32(statResult, 4);
            value.Time = Utilities.FromUnixTimeSeconds(BitConverter.ToInt32(statResult, 8));
        }
    }
}
