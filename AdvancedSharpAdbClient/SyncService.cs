// <copyright file="SyncService.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace AdvancedSharpAdbClient
{
    /// <summary>
    /// Provides access to the sync service running on the Android device. Allows you to
    /// list, download and upload files on the device.
    /// </summary>
    /// <example>
    /// <para>To send files to or receive files from your Android device, you can use the following code:</para>
    /// <code>
    /// void DownloadFile()
    /// {
    ///     var device = new AdbClient().GetDevices().First();
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
    ///     var device = new AdbClient().GetDevices().First();
    ///
    ///     using (SyncService service = new SyncService(new AdbSocket(), device))
    ///     using (Stream stream = File.OpenRead(@"C:\MyFile.txt"))
    ///     {
    ///         service.Push(stream, "/data/MyFile.txt", null, CancellationToken.None);
    ///     }
    /// }
    /// </code>
    /// </example>
    public partial class SyncService : ISyncService
    {
        /// <summary>
        /// The maximum length of a path on the remote device.
        /// </summary>
        protected const int MaxPathLength = 1024;

        /// <inheritdoc/>
        public event EventHandler<SyncProgressChangedEventArgs>? SyncProgressChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncService"/> class.
        /// </summary>
        /// <param name="device">The device on which to interact with the files.</param>
        public SyncService(DeviceData device)
            : this(Factories.AdbSocketFactory(new IPEndPoint(IPAddress.Loopback, AdbClient.AdbServerPort)), device)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncService"/> class.
        /// </summary>
        /// <param name="client">A connection to an adb server.</param>
        /// <param name="device">The device on which to interact with the files.</param>
        public SyncService(IAdbClient client, DeviceData device)
            : this(Factories.AdbSocketFactory(client.EndPoint), device)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncService"/> class.
        /// </summary>
        /// <param name="endPoint">The <see cref="EndPoint"/> at which the adb server is listening.</param>
        /// <param name="device">The device on which to interact with the files.</param>
        public SyncService(EndPoint endPoint, DeviceData device)
            : this(Factories.AdbSocketFactory(endPoint), device)
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
        public DeviceData Device { get; }

        /// <summary>
        /// Gets the <see cref="IAdbSocket"/> that enables the connection with the adb server.
        /// </summary>
        public IAdbSocket Socket { get; protected set; }

        /// <inheritdoc/>
        public bool IsOpen => Socket != null && Socket.Connected;

        /// <inheritdoc/>
        public virtual void Open()
        {
            // target a specific device
            Socket.SetDevice(Device);

            Socket.SendAdbRequest("sync:");
            _ = Socket.ReadAdbResponse();
        }

        /// <inheritdoc/>
        public virtual void Reopen()
        {
            Socket.Reconnect(true);
            Open();
        }

        /// <inheritdoc/>
        public virtual void Push(Stream stream, string remotePath, int permissions, DateTimeOffset timestamp, IProgress<int>? progress = null, in bool isCancelled = false)
        {
            ExceptionExtensions.ThrowIfNull(stream);
            ExceptionExtensions.ThrowIfNull(remotePath);

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
            while (!isCancelled)
            {
                // read up to SYNC_DATA_MAX
#if HAS_BUFFERS
                int read = stream.Read(buffer.AsSpan(headerSize, maxDataSize));
#else
                int read = stream.Read(buffer, headerSize, maxDataSize);
#endif
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
#if HAS_BUFFERS
                Socket.Send(buffer.AsSpan(startPosition, read + dataBytes.Length + lengthBytes.Length));
#else
                Socket.Send(buffer, startPosition, read + dataBytes.Length + lengthBytes.Length);
#endif

                SyncProgressChanged?.Invoke(this, new SyncProgressChangedEventArgs(totalBytesRead, totalBytesToProcess));

                // Let the caller know about our progress, if requested
                if (progress != null && totalBytesToProcess != 0)
                {
                    progress.Report((int)(100.0 * totalBytesRead / totalBytesToProcess));
                }
            }

            // create the DONE message
            int time = (int)timestamp.ToUnixTimeSeconds();
            Socket.SendSyncRequest(SyncCommand.DONE, time);

            // read the result, in a byte array containing 2 int
            // (id, size)
            SyncCommand result = Socket.ReadSyncResponse();

            if (result == SyncCommand.FAIL)
            {
                string message = Socket.ReadSyncString();

                throw new AdbException(message);
            }
            else if (result != SyncCommand.OKAY)
            {
                throw new AdbException($"The server sent an invalid response {result}");
            }
        }

        /// <inheritdoc/>
        public virtual void Pull(string remoteFilePath, Stream stream, IProgress<int>? progress = null, in bool isCancelled = false)
        {
            ExceptionExtensions.ThrowIfNull(remoteFilePath);
            ExceptionExtensions.ThrowIfNull(stream);

            // Get file information, including the file size, used to calculate the total amount of bytes to receive.
            FileStatistics stat = Stat(remoteFilePath);
            long totalBytesToProcess = stat.Size;
            long totalBytesRead = 0;

            byte[] buffer = new byte[MaxBufferSize];

            Socket.SendSyncRequest(SyncCommand.RECV, remoteFilePath);

            while (!isCancelled)
            {
                SyncCommand response = Socket.ReadSyncResponse();

                if (response == SyncCommand.DONE)
                {
                    break;
                }
                else if (response == SyncCommand.FAIL)
                {
                    string message = Socket.ReadSyncString();
                    throw new AdbException($"Failed to pull '{remoteFilePath}'. {message}");
                }
                else if (response != SyncCommand.DATA)
                {
                    throw new AdbException($"The server sent an invalid response {response}");
                }

                // The first 4 bytes contain the length of the data packet
                byte[] reply = new byte[4];
                _ = Socket.Read(reply);

                if (!BitConverter.IsLittleEndian)
                {
                    Array.Reverse(reply);
                }

                int size =
#if HAS_BUFFERS
                    BitConverter.ToInt32(reply);
#else
                    BitConverter.ToInt32(reply, 0);
#endif

                if (size > MaxBufferSize)
                {
                    throw new AdbException($"The adb server is sending {size} bytes of data, which exceeds the maximum chunk size {MaxBufferSize}");
                }

                // now read the length we received
#if HAS_BUFFERS
                _ = Socket.Read(buffer.AsSpan(0, size));
                stream.Write(buffer.AsSpan(0, size));
#else
                _ = Socket.Read(buffer, size);
                stream.Write(buffer, size);
#endif
                totalBytesRead += size;

                SyncProgressChanged?.Invoke(this, new SyncProgressChangedEventArgs(totalBytesRead, totalBytesToProcess));

                // Let the caller know about our progress, if requested
                if (progress != null && totalBytesToProcess != 0)
                {
                    progress.Report((int)(100.0 * totalBytesRead / totalBytesToProcess));
                }
            }
        }

        /// <inheritdoc/>
        public virtual FileStatistics Stat(string remotePath)
        {
            // create the stat request message.
            Socket.SendSyncRequest(SyncCommand.STAT, remotePath);

            SyncCommand response = Socket.ReadSyncResponse();
            if (response != SyncCommand.STAT)
            {
                throw new AdbException($"The server returned an invalid sync response {response}.");
            }

            // read the result, in a byte array containing 3 int
            // (mode, size, time)
            FileStatistics value = ReadStatistics();
            value.Path = remotePath;

            return value;
        }

        /// <inheritdoc/>
        public virtual IEnumerable<FileStatistics> GetDirectoryListing(string remotePath)
        {
            bool isLocked = false;

            start:
            // create the stat request message.
            Socket.SendSyncRequest(SyncCommand.LIST, remotePath);

            while (true)
            {
                SyncCommand response = Socket.ReadSyncResponse();

                if (response == 0)
                {
                    if (isLocked)
                    {
                        throw new AdbException("The server returned an empty sync response.");
                    }
                    else
                    {
                        Reopen();
                        isLocked = true;
                        goto start;
                    }
                }
                else if (response == SyncCommand.DONE)
                {
                    break;
                }
                else if (response != SyncCommand.DENT)
                {
                    throw new AdbException($"The server returned an invalid sync response {response}.");
                }

                FileStatistics entry = ReadStatistics();
                entry.Path = Socket.ReadSyncString();

                yield return entry;
                isLocked = true;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Socket != null)
                {
                    Socket.Dispose();
                    Socket = null!;
                }
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Reads the statistics of a file from the socket.
        /// </summary>
        /// <returns>A <see cref="FileStatistics"/> object that contains information about the file.</returns>
        protected FileStatistics ReadStatistics()
        {
            byte[] statResult = new byte[12];
            _ = Socket.Read(statResult);

            int index = 0;

            return new FileStatistics
            {
                FileType = (UnixFileType)ReadInt32(in statResult),
                Size = ReadInt32(in statResult),
                Time = DateTimeExtensions.FromUnixTimeSeconds(ReadInt32(in statResult))
            };

            int ReadInt32(in byte[] data) => data[index++] | (data[index++] << 8) | (data[index++] << 16) | (data[index++] << 24);
        }
    }
}
