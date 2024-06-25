// <copyright file="SyncService.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;

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
    [DebuggerDisplay($"{nameof(SyncService)} \\{{ {nameof(IsOpen)} = {{{nameof(IsOpen)}}}, {nameof(Device)} = {{{nameof(Device)}}}, {nameof(Socket)} = {{{nameof(Socket)}}}, {nameof(MaxBufferSize)} = {{{nameof(MaxBufferSize)}}} }}")]
    public partial class SyncService : ISyncService, ICloneable<SyncService>, ICloneable
#if HAS_WINRT
        , ISyncService.IWinRT
#endif
    {
        /// <summary>
        /// The maximum length of a path on the remote device.
        /// </summary>
        protected const int MaxPathLength = 1024;

        /// <summary>
        /// <see langword="true"/> if the <see cref="SyncService"/> is out of date; otherwise, <see langword="false"/>.
        /// </summary>
        /// <remarks>Need to invoke <see cref="Reopen"/> before using the <see cref="SyncService"/> again.</remarks>
        protected internal bool IsOutdate = false;

        /// <summary>
        /// <see langword="true"/> if the <see cref="SyncService"/> is processing; otherwise, <see langword="false"/>.
        /// </summary>
        /// <remarks>Recommend to <see cref="Clone()"/> a new <see cref="ISyncService"/> or wait until the process is finished.</remarks>
        protected internal bool IsProcessing = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncService"/> class.
        /// </summary>
        /// <param name="device">The device on which to interact with the files.</param>
        public SyncService(DeviceData device)
            : this(Factories.AdbSocketFactory(AdbClient.AdbServerEndPoint), device)
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
            Device = DeviceData.EnsureDevice(ref device);
            Socket = socket ?? throw new ArgumentNullException(nameof(socket));
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
        public bool IsOpen => Socket?.Connected == true;

        /// <inheritdoc/>
        /// <remarks>This method has been invoked by the constructor.
        /// Do not use it unless you have closed the connection.
        /// Use <see cref="Reopen"/> to reopen the connection.</remarks>
        public void Open()
        {
            // target a specific device
            Socket.SetDevice(Device);

            Socket.SendAdbRequest("sync:");
            _ = Socket.ReadAdbResponse();

            IsOutdate = false;
        }

        /// <inheritdoc/>
        public void Reopen()
        {
            Socket.Reconnect(true);
            Open();
        }

        /// <inheritdoc/>
        public virtual void Push(Stream stream, string remotePath, UnixFileStatus permission, DateTimeOffset timestamp, Action<SyncProgressChangedEventArgs>? callback = null, in bool isCancelled = false)
        {
            if (IsProcessing) { throw new InvalidOperationException($"The {nameof(SyncService)} is currently processing a request. Please {nameof(Clone)} a new {nameof(ISyncService)} or wait until the process is finished."); }

            ExceptionExtensions.ThrowIfNull(stream);
            ExceptionExtensions.ThrowIfNull(remotePath);

            if (remotePath.Length > MaxPathLength)
            {
                throw new ArgumentOutOfRangeException(nameof(remotePath), $"The remote path {remotePath} exceeds the maximum path size {MaxPathLength}");
            }

            if (IsOutdate) { Reopen(); }

            try
            {
                Socket.SendSyncRequest(SyncCommand.SEND, remotePath, permission);
                IsProcessing = true;

                // create the buffer used to read.
                // we read max SYNC_DATA_MAX.
                byte[] buffer = new byte[MaxBufferSize];

                // We need 4 bytes of the buffer to send the 'DATA' command,
                // and an additional X bytes to inform how much data we are
                // sending.
                byte[] dataBytes = SyncCommand.DATA.GetBytes();
                byte[] lengthBytes = BitConverter.GetBytes(MaxBufferSize);
                int headerSize = dataBytes.Length + lengthBytes.Length;
                int reservedHeaderSize = headerSize;
                int maxDataSize = MaxBufferSize - reservedHeaderSize;
                lengthBytes = BitConverter.GetBytes(maxDataSize);

                // Try to get the total amount of bytes to transfer. This is not always possible, for example,
                // for forward-only streams.
                long totalBytesToProcess = stream.CanSeek ? stream.Length : 0;
                long totalBytesRead = 0;

                int read;
                // look while there is something to read
                while (!isCancelled && (read =
#if HAS_BUFFERS
                    stream.Read(buffer.AsSpan(headerSize, maxDataSize))
#else
                    stream.Read(buffer, headerSize, maxDataSize)
#endif
                    ) > 0)
                {
                    // read up to SYNC_DATA_MAX
                    totalBytesRead += read;

                    if (read != maxDataSize)
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
                    // Let the caller know about our progress, if requested
                    callback?.Invoke(new SyncProgressChangedEventArgs(totalBytesRead, totalBytesToProcess));
                }

                // create the DONE message
                int time = (int)timestamp.ToUnixTimeSeconds();
                Socket.SendSyncRequest(SyncCommand.DONE, time);

                // read the result, in a byte array containing 2 int
                // (id, size)
                SyncCommand result = Socket.ReadSyncResponse();

                switch (result)
                {
                    case SyncCommand.FAIL:
                        string message = Socket.ReadSyncString();
                        throw new AdbException(message);
                    case not SyncCommand.OKAY:
                        throw new AdbException($"The server sent an invalid response {result}");
                }
            }
            finally
            {
                IsOutdate = true;
                IsProcessing = false;
            }
        }

        /// <inheritdoc/>
        public virtual void Pull(string remoteFilePath, Stream stream, Action<SyncProgressChangedEventArgs>? callback = null, in bool isCancelled = false)
        {
            if (IsProcessing) { throw new InvalidOperationException($"The {nameof(SyncService)} is currently processing a request. Please {nameof(Clone)} a new {nameof(ISyncService)} or wait until the process is finished."); }

            ExceptionExtensions.ThrowIfNull(remoteFilePath);
            ExceptionExtensions.ThrowIfNull(stream);

            if (IsOutdate) { Reopen(); }

            // Gets file information, including the file size, used to calculate the total amount of bytes to receive.
            FileStatistics stat = Stat(remoteFilePath);
            int totalBytesToProcess = stat.Size;
            long totalBytesRead = 0;

            byte[] buffer = new byte[MaxBufferSize];

            try
            {
                Socket.SendSyncRequest(SyncCommand.RECV, remoteFilePath);
                IsProcessing = true;

                while (!isCancelled)
                {
                    SyncCommand response = Socket.ReadSyncResponse();

                    switch (response)
                    {
                        case SyncCommand.DONE:
                            goto finish;
                        case SyncCommand.FAIL:
                            string message = Socket.ReadSyncString();
                            throw new AdbException($"Failed to pull '{remoteFilePath}'. {message}");
                        case not SyncCommand.DATA:
                            throw new AdbException($"The server sent an invalid response {response}");
                    }

                    // The first 4 bytes contain the length of the data packet
                    byte[] reply = new byte[4];
                    _ = Socket.Read(reply);

                    int size = reply[0] | (reply[1] << 8) | (reply[2] << 16) | (reply[3] << 24);

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
                    stream.Write(buffer, 0, size);
#endif
                    totalBytesRead += size;

                    // Let the caller know about our progress, if requested
                    callback?.Invoke(new SyncProgressChangedEventArgs(totalBytesRead, totalBytesToProcess));
                }

            finish: return;
            }
            finally
            {
                IsOutdate = true;
                IsProcessing = false;
            }
        }

        /// <inheritdoc/>
        public FileStatistics Stat(string remotePath)
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
        public IEnumerable<FileStatistics> GetDirectoryListing(string remotePath)
        {
            if (IsProcessing) { throw new InvalidOperationException($"The {nameof(SyncService)} is currently processing a request. Please {nameof(Clone)} a new {nameof(ISyncService)} or wait until the process is finished."); }
            if (IsOutdate) { Reopen(); }
            bool isLocked = false;

            try
            {
            start:
                // create the stat request message.
                Socket.SendSyncRequest(SyncCommand.LIST, remotePath);
                IsProcessing = true;

                while (true)
                {
                    SyncCommand response = Socket.ReadSyncResponse();

                    switch (response)
                    {
                        case 0 when isLocked:
                            throw new AdbException("The server returned an empty sync response.");
                        case 0:
                            Reopen();
                            isLocked = true;
                            goto start;
                        case SyncCommand.DONE:
                            goto finish;
                        case not SyncCommand.DENT:
                            throw new AdbException($"The server returned an invalid sync response {response}.");
                    }

                    FileStatistics entry = ReadStatistics();
                    entry.Path = Socket.ReadSyncString();

                    yield return entry;
                    isLocked = true;
                }

            finish:
                yield break;
            }
            finally
            {
                IsOutdate = true;
                IsProcessing = false;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && Socket != null)
            {
                Socket.Dispose();
                Socket = null!;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public override string ToString() =>
            new StringBuilder(nameof(SyncService))
                .Append(" { ")
                .Append(nameof(Socket))
                .Append(" = ")
                .Append(Socket)
                .Append(", ")
                .Append(nameof(Device))
                .Append(" = ")
                .Append(Device)
                .Append(" }")
                .ToString();

        /// <summary>
        /// Creates a new <see cref="SyncService"/> object that is a copy of the current instance with new <see cref="Device"/>.
        /// </summary>
        /// <param name="device">The new <see cref="Device"/> to use.</param>
        /// <returns>A new <see cref="SyncService"/> object that is a copy of this instance with new <see cref="Device"/>.</returns>
        public virtual SyncService Clone(DeviceData device) =>
            Socket is ICloneable<IAdbSocket> cloneable
                ? new SyncService(cloneable.Clone(), device)
                : throw new NotSupportedException($"{Socket.GetType()} does not support cloning.");

        /// <inheritdoc/>
        public SyncService Clone() => Clone(Device);

        /// <inheritdoc/>
        object ICloneable.Clone() => Clone(Device);

        /// <summary>
        /// Reads the statistics of a file from the socket.
        /// </summary>
        /// <returns>A <see cref="FileStatistics"/> object that contains information about the file.</returns>
        protected FileStatistics ReadStatistics()
        {
#if HAS_BUFFERS
            Span<byte> statResult = stackalloc byte[12];
            _ = Socket.Read(statResult);
            return EnumerableBuilder.FileStatisticsCreator(statResult);
#else
            byte[] statResult = new byte[12];
            _ = Socket.Read(statResult);
            int index = 0;
            return new FileStatistics
            {
                FileMode = (UnixFileStatus)ReadInt32(statResult),
                Size = ReadInt32(statResult),
                Time = DateTimeExtensions.FromUnixTimeSeconds(ReadInt32(statResult))
            };
            int ReadInt32(byte[] data) => data[index++] | (data[index++] << 8) | (data[index++] << 16) | (data[index++] << 24);
#endif
        }
    }
}
