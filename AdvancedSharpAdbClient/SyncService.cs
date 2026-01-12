// <copyright file="SyncService.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;

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
    [DebuggerDisplay($"{{{nameof(GetType)}().{nameof(Type.ToString)}(),nq}} \\{{ {nameof(IsOpen)} = {{{nameof(IsOpen)}}}, {nameof(Device)} = {{{nameof(Device)}}}, {nameof(Socket)} = {{{nameof(Socket)}}}, {nameof(MaxBufferSize)} = {{{nameof(MaxBufferSize)}}} }}")]
    public partial class SyncService : ISyncService, IInternalCloneable<SyncService>
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
            Device = DeviceData.EnsureDevice(device);
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
        public virtual void Push(Stream stream, string remotePath, UnixFileStatus permission, DateTimeOffset timestamp, Action<SyncProgressChangedEventArgs>? callback = null, bool useV2 = false, in bool isCancelled = false)
        {
            if (IsProcessing) { throw new InvalidOperationException($"The {nameof(SyncService)} is currently processing a request. Please {nameof(Clone)} a new {nameof(ISyncService)} or wait until the process is finished."); }

            ArgumentNullException.ThrowIfNull(stream);
            ArgumentNullException.ThrowIfNull(remotePath);

            if (remotePath.Length > MaxPathLength)
            {
                throw new ArgumentOutOfRangeException(nameof(remotePath), $"The remote path {remotePath} exceeds the maximum path size {MaxPathLength}");
            }

            if (IsOutdate) { Reopen(); }

            try
            {
                if (useV2)
                {
                    Socket.SendSyncRequest(SyncCommand.SND2, remotePath);
                    IsProcessing = true;
                    Socket.SendSyncRequest(SyncCommand.SND2, permission, SyncFlags.None);
                }
                else
                {
                    Socket.SendSyncRequest(SyncCommand.SEND, remotePath, permission);
                    IsProcessing = true;
                }

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
#if COMP_NETSTANDARD2_1
                    Socket.Send(buffer.AsSpan(startPosition, read + dataBytes.Length + lengthBytes.Length));
#else
                    Socket.Send(buffer, startPosition, read + dataBytes.Length + lengthBytes.Length);
#endif
                    // Let the caller know about our progress, if requested
                    callback?.Invoke(new SyncProgressChangedEventArgs((ulong)totalBytesRead, (ulong)totalBytesToProcess));
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
        public virtual void Pull(string remoteFilePath, Stream stream, Action<SyncProgressChangedEventArgs>? callback = null, bool useV2 = false, in bool isCancelled = false)
        {
            if (IsProcessing) { throw new InvalidOperationException($"The {nameof(SyncService)} is currently processing a request. Please {nameof(Clone)} a new {nameof(ISyncService)} or wait until the process is finished."); }

            ArgumentNullException.ThrowIfNull(remoteFilePath);
            ArgumentNullException.ThrowIfNull(stream);

            if (IsOutdate) { Reopen(); }

            // Gets file information, including the file size, used to calculate the total amount of bytes to receive.
            ulong totalBytesToProcess = this.Stat(remoteFilePath, useV2).Size;
            ulong totalBytesRead = 0;

            byte[] buffer = new byte[MaxBufferSize];

            try
            {
                if (useV2)
                {
                    Socket.SendSyncRequest(SyncCommand.RCV2, remoteFilePath);
                    IsProcessing = true;
                    Socket.SendSyncRequest(SyncCommand.RCV2, (int)SyncFlags.None);
                }
                else
                {
                    Socket.SendSyncRequest(SyncCommand.RECV, remoteFilePath);
                    IsProcessing = true;
                }

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

                    int size = BitConverter.ToInt32(reply);

                    if (size > MaxBufferSize)
                    {
                        throw new AdbException($"The adb server is sending {size} bytes of data, which exceeds the maximum chunk size {MaxBufferSize}");
                    }

                    // now read the length we received
#if COMP_NETSTANDARD2_1
                    _ = Socket.Read(buffer.AsSpan(0, size));
#else
                    _ = Socket.Read(buffer, size);
#endif
#if HAS_BUFFERS
                    stream.Write(buffer.AsSpan(0, size));
#else
                    stream.Write(buffer, 0, size);
#endif
                    totalBytesRead += (uint)size;

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
        public FileStatisticsEx StatEx(string remotePath)
        {
            // create the stat request message.
            Socket.SendSyncRequest(SyncCommand.STA2, remotePath);

            SyncCommand response = Socket.ReadSyncResponse();
            if (response != SyncCommand.STA2)
            {
                throw new AdbException($"The server returned an invalid sync response {response}.");
            }

            FileStatisticsEx value = ReadStatisticsV2();
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

        /// <inheritdoc/>
        public IEnumerable<FileStatisticsEx> GetDirectoryListingEx(string remotePath)
        {
            if (IsProcessing) { throw new InvalidOperationException($"The {nameof(SyncService)} is currently processing a request. Please {nameof(Clone)} a new {nameof(ISyncService)} or wait until the process is finished."); }
            if (IsOutdate) { Reopen(); }
            bool isLocked = false;

            try
            {
            start:
                // create the stat request message.
                Socket.SendSyncRequest(SyncCommand.LIS2, remotePath);
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
                        case not SyncCommand.DNT2:
                            throw new AdbException($"The server returned an invalid sync response {response}.");
                    }

                    FileStatisticsEx entry = ReadStatisticsV2();
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
        public override string ToString() => $"{GetType()} {{ {nameof(Socket)} = {Socket}, {nameof(Device)} = {Device} }}";

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
#if COMP_NETSTANDARD2_1
            Span<byte> statResult = stackalloc byte[FileStatisticsData.Length];
            _ = Socket.Read(statResult);
            FileStatisticsData data = EnumerableBuilder.FileStatisticsDataCreator(statResult);
            return new FileStatistics(data);
#else
            byte[] statResult = new byte[FileStatisticsData.Length];
            _ = Socket.Read(statResult);
            ref FileStatisticsData data = ref Unsafe.As<byte, FileStatisticsData>(ref statResult[0]);
            return new FileStatistics(data);
#endif
        }

        /// <summary>
        /// Reads the statistics of a file from the socket (v2).
        /// </summary>
        /// <returns>A <see cref="FileStatisticsEx"/> object that contains information about the file.</returns>
        protected FileStatisticsEx ReadStatisticsV2()
        {
#if COMP_NETSTANDARD2_1
            Span<byte> statResult = stackalloc byte[FileStatisticsDataEx.Length];
            _ = Socket.Read(statResult);
            FileStatisticsDataEx data = EnumerableBuilder.FileStatisticsDataV2Creator(statResult);
            return new FileStatisticsEx(data);
#else
            byte[] statResult = new byte[FileStatisticsDataEx.Length];
            _ = Socket.Read(statResult);
            ref FileStatisticsDataEx data = ref Unsafe.As<byte, FileStatisticsDataEx>(ref statResult[0]);
            return new FileStatisticsEx(data);
#endif
        }
    }
}
