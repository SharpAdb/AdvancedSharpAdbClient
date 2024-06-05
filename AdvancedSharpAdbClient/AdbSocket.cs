// <copyright file="AdbSocket.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace AdvancedSharpAdbClient
{
    /// <summary>
    /// <para>Implements a client for the Android Debug Bridge client-server protocol. Using the client, you
    /// can send messages to and receive messages from the Android Debug Bridge.</para>
    /// <para>The <see cref="AdbSocket"/> class implements the raw messaging protocol; that is,
    /// sending and receiving messages. For interacting with the services the Android Debug
    /// Bridge exposes, use the <see cref="AdbClient"/>.</para>
    /// <para>For more information about the protocol that is implemented here, see chapter
    /// II Protocol Details, section 1. Client &lt;-&gt;Server protocol at
    /// <see href="https://android.googlesource.com/platform/system/core/+/master/adb/OVERVIEW.TXT"/>.</para>
    /// </summary>
    /// <param name="socket">The <see cref="ITcpSocket"/> at which the Android Debug Bridge is listening for clients.</param>
    /// <param name="logger">The logger to use when logging.</param>
    [DebuggerDisplay($"{nameof(AdbSocket)} \\{{ {nameof(Connected)} = {{{nameof(Connected)}}}, {nameof(Socket)} = {{{nameof(Socket)}}} }}")]
    public partial class AdbSocket(ITcpSocket socket, ILogger<AdbSocket>? logger = null) : IAdbSocket, ICloneable<AdbSocket>, ICloneable
    {
        /// <summary>
        /// The logger to use when logging messages.
        /// </summary>
        private readonly ILogger<AdbSocket> logger = logger ?? LoggerProvider.CreateLogger<AdbSocket>();

        /// <summary>
        /// Initializes a new instance of the <see cref="AdbSocket"/> class.
        /// </summary>
        /// <param name="logger">The logger to use when logging.</param>
        public AdbSocket(ILogger<AdbSocket>? logger = null)
            : this(AdbClient.AdbServerEndPoint, logger)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdbSocket"/> class.
        /// </summary>
        /// <param name="endPoint">The <see cref="EndPoint"/> at which the Android Debug Bridge is listening for clients.</param>
        /// <param name="logger">The logger to use when logging.</param>
        public AdbSocket(EndPoint endPoint, ILogger<AdbSocket>? logger = null)
            : this(CreateTcpSocket(endPoint), logger)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdbSocket"/> class.
        /// </summary>
        /// <param name="host">The host address at which the Android Debug Bridge is listening for clients.</param>
        /// <param name="port">The port at which the Android Debug Bridge is listening for clients.</param>
        /// <param name="logger">The logger to use when logging.</param>
        public AdbSocket(string host, int port, ILogger<AdbSocket>? logger = null)
            : this(Extensions.CreateDnsEndPoint(host, port), logger)
        {
        }

        /// <summary>
        /// Gets or sets the size of the receive buffer
        /// </summary>
        public static int ReceiveBufferSize { get; set; } = 40960;

        /// <summary>
        /// Gets or sets the size of the write buffer.
        /// </summary>
        public static int WriteBufferSize { get; set; } = 1024;

        /// <summary>
        /// Gets the underlying TCP socket that manages the connection with the ADB server.
        /// </summary>
        public ITcpSocket Socket { get; } = socket ?? throw new ArgumentNullException(nameof(socket));

        /// <summary>
        /// Determines whether the specified reply is okay.
        /// </summary>
        /// <param name="reply">The reply.</param>
        /// <returns><see langword="true"/> if the specified reply is okay; otherwise, <see langword="false"/>.</returns>
        public static bool IsOkay(byte[] reply) => AdbClient.Encoding.GetString(reply).Equals("OKAY");

#if HAS_BUFFERS
        /// <summary>
        /// Determines whether the specified reply is okay.
        /// </summary>
        /// <param name="reply">The reply.</param>
        /// <returns><see langword="true"/> if the specified reply is okay; otherwise, <see langword="false"/>.</returns>
        public static bool IsOkay(ReadOnlySpan<byte> reply) => AdbClient.Encoding.GetString(reply).Equals("OKAY");
#endif

        /// <inheritdoc/>
        public bool Connected => Socket.Connected;

        /// <inheritdoc/>
        public void Reconnect(bool isForce = false) => Socket.Reconnect(isForce);

        /// <inheritdoc/>
        public virtual void Send(byte[] data)
        {
            try
            {
                int count = Socket.Send(data, SocketFlags.None);
                if (count < 0)
                {
                    throw new AdbException("channel EOF");
                }
            }
            catch (SocketException sex)
            {
                logger.LogError(sex, sex.Message);
                throw;
            }
        }

        /// <inheritdoc/>
        public virtual void Send(byte[] data, int length)
        {
            try
            {
                int count = Socket.Send(data, length != -1 ? length : data.Length, SocketFlags.None);
                if (count < 0)
                {
                    throw new AdbException("channel EOF");
                }
            }
            catch (SocketException sex)
            {
                logger.LogError(sex, sex.Message);
                throw;
            }
        }

        /// <inheritdoc/>
        public virtual void Send(byte[] data, int offset, int length)
        {
            try
            {
                int count = Socket.Send(data, offset, length != -1 ? length : data.Length, SocketFlags.None);
                if (count < 0)
                {
                    throw new AdbException("channel EOF");
                }
            }
            catch (SocketException sex)
            {
                logger.LogError(sex, sex.Message);
                throw;
            }
        }

        /// <inheritdoc/>
        public void SendSyncRequest(SyncCommand command, string path, UnixFileStatus permissions) =>
            SendSyncRequest(command, $"{path},{(int)permissions.GetPermissions()}");

        /// <inheritdoc/>
        public void SendSyncRequest(SyncCommand command, string path)
        {
            ExceptionExtensions.ThrowIfNull(path);
            byte[] pathBytes = AdbClient.Encoding.GetBytes(path);
            SendSyncRequest(command, pathBytes.Length);
            _ = Write(pathBytes);
        }

        /// <inheritdoc/>
        public void SendSyncRequest(SyncCommand command, int length)
        {
            // The message structure is:
            // First four bytes: command
            // Next four bytes: length of the path
            // Final bytes: path
            byte[] commandBytes = command.GetBytes();

            byte[] lengthBytes = BitConverter.GetBytes(length);

            if (!BitConverter.IsLittleEndian)
            {
                // Convert from big endian to little endian
                Array.Reverse(lengthBytes);
            }

            _ = Write(commandBytes);
            _ = Write(lengthBytes);
        }

        /// <inheritdoc/>
        public void SendAdbRequest(string request)
        {
            byte[] data = AdbClient.FormAdbRequest(request);
            if (!Write(data))
            {
                throw new IOException($"Failed sending the request '{request}' to ADB");
            }
        }

        /// <inheritdoc/>
        public virtual int Read(byte[] data) =>
#if HAS_BUFFERS
            Read(data.AsSpan());
#else
            Read(data, 0, data.Length);
#endif

        /// <inheritdoc/>
        public virtual int Read(byte[] data, int length) =>
#if HAS_BUFFERS
            Read(data.AsSpan(0, length));
#else
            Read(data, 0, length);
#endif

        /// <inheritdoc/>
        public virtual int Read(byte[] data, int offset, int length)
        {
            ExceptionExtensions.ThrowIfNull(data);
            ExceptionExtensions.ThrowIfNegative(offset);

            length = length != -1 ? length : data.Length;
            ExceptionExtensions.ThrowIfLessThan(data.Length, length, nameof(data));

            int count = -1;
            int totalRead = offset;

            while (count != 0 && totalRead < length)
            {
                try
                {
                    int left = length - totalRead;
                    int bufferLength = left < ReceiveBufferSize ? left : ReceiveBufferSize;

                    count = Socket.Receive(data, totalRead, bufferLength, SocketFlags.None);

                    if (count < 0)
                    {
                        logger.LogError("read: channel EOF");
                        throw new AdbException("EOF");
                    }
                    else if (count == 0)
                    {
                        logger.LogInformation("DONE with Read");
                    }
                    else
                    {
                        totalRead += count;
                    }
                }
                catch (SocketException ex)
                {
                    throw new AdbException($"An error occurred while receiving data from the adb server: {ex.Message}.", ex);
                }
            }

            return totalRead;
        }

        /// <inheritdoc/>
        public virtual string ReadString()
        {
            // The first 4 bytes contain the length of the string
            byte[] reply = new byte[4];
            int read = Read(reply);

            if (read == 0)
            {
                // There is no data to read
                return string.Empty;
            }

            // Convert the bytes to a hex string
            string lenHex = AdbClient.Encoding.GetString(reply);
            int len = int.Parse(lenHex, NumberStyles.HexNumber);

            // And get the string
            reply = new byte[len];
            _ = Read(reply);

            string value = AdbClient.Encoding.GetString(reply);
            return value;
        }

        /// <inheritdoc/>
        public virtual string ReadSyncString()
        {
            // The first 4 bytes contain the length of the string
            byte[] reply = new byte[4];
            int read = Read(reply);

            if (read == 0)
            {
                // There is no data to read
                return string.Empty;
            }

            // Get the length of the string
            int len = reply[0] | (reply[1] << 8) | (reply[2] << 16) | (reply[3] << 24);

            // And get the string
            reply = new byte[len];
            _ = Read(reply);

            string value = AdbClient.Encoding.GetString(reply);
            return value;
        }

        /// <inheritdoc/>
        public SyncCommand ReadSyncResponse()
        {
            byte[] data = new byte[4];
            _ = Read(data);
            return SyncCommandConverter.GetCommand(data);
        }

        /// <inheritdoc/>
        public AdbResponse ReadAdbResponse()
        {
            AdbResponse response = ReadAdbResponseInner();

            if (!response.IOSuccess || !response.Okay)
            {
                Socket.Dispose();
                throw new AdbException($"An error occurred while reading a response from ADB: {response.Message}", response);
            }

            return response;
        }

#if HAS_BUFFERS
        /// <inheritdoc/>
        public virtual void Send(ReadOnlySpan<byte> data)
        {
            try
            {
                int count = Socket.Send(data, SocketFlags.None);
                if (count < 0)
                {
                    throw new AdbException("channel EOF");
                }
            }
            catch (SocketException sex)
            {
                logger.LogError(sex, sex.Message);
                throw;
            }
        }

        /// <inheritdoc/>
        public virtual int Read(Span<byte> data)
        {
            int count = -1;
            int totalRead = 0;
            int length = data.Length;

            while (count != 0 && totalRead < length)
            {
                try
                {
                    int left = length - totalRead;
                    int bufferLength = left < ReceiveBufferSize ? left : ReceiveBufferSize;

                    count = Socket.Receive(data.Slice(totalRead, bufferLength), SocketFlags.None);

                    if (count < 0)
                    {
                        logger.LogError("read: channel EOF");
                        throw new AdbException("EOF");
                    }
                    else if (count == 0)
                    {
                        logger.LogInformation("DONE with Read");
                    }
                    else
                    {
                        totalRead += count;
                    }
                }
                catch (SocketException ex)
                {
                    throw new AdbException($"An error occurred while receiving data from the adb server: {ex.Message}.", ex);
                }
            }

            return totalRead;
        }
#endif

        /// <inheritdoc/>
        public virtual Stream GetShellStream()
        {
            Stream stream = Socket.GetStream();
            return new ShellStream(stream, closeStream: true);
        }

        /// <inheritdoc/>
        public void SetDevice(DeviceData device)
        {
            // if the device is not null, then we first tell adb we're looking to talk
            // to a specific device
            if (device != null)
            {
                if (uint.TryParse(device.TransportId, out uint tid))
                {
                    SendAdbRequest($"host:transport-id:{tid}");
                }
                else
                {
                    SendAdbRequest($"host:transport:{device.Serial}");
                }

                try
                {
                    AdbResponse response = ReadAdbResponse();
                }
                catch (AdbException e)
                {
                    if (string.Equals("device not found", e.AdbError, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new DeviceNotFoundException(device.Serial);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Write until all data in "data" is written or the connection fails or times out.
        /// </summary>
        /// <param name="data">The data to send.</param>
        /// <returns>Returns <see langword="true"/> if all data was written; otherwise, <see langword="false"/>.</returns>
        /// <remarks>This uses the default time out value.</remarks>
        protected virtual bool Write(byte[] data)
        {
            try
            {
                Send(data);
            }
            catch (IOException e)
            {
                logger.LogError(e, e.Message);
                return false;
            }

            return true;
        }

#if HAS_BUFFERS
        /// <summary>
        /// Write until all data in "data" is written or the connection fails or times out.
        /// </summary>
        /// <param name="data">The data to send.</param>
        /// <returns>Returns <see langword="true"/> if all data was written; otherwise, <see langword="false"/>.</returns>
        /// <remarks>This uses the default time out value.</remarks>
        protected virtual bool Write(ReadOnlySpan<byte> data)
        {
            try
            {
                Send(data);
            }
            catch (IOException e)
            {
                logger.LogError(e, e.Message);
                return false;
            }

            return true;
        }
#endif

        /// <summary>
        /// Reads the response from ADB after a command.
        /// </summary>
        /// <returns>A <see cref="AdbResponse"/> that represents the response received from ADB.</returns>
        protected virtual AdbResponse ReadAdbResponseInner()
        {
            byte[] reply = new byte[4];
            Read(reply);

            if (IsOkay(reply))
            {
                return AdbResponse.OK;
            }
            else
            {
                string message = ReadString();
                logger.LogError("Got reply '{0}', diag='{1}'", ReplyToString(reply), message);
                return AdbResponse.FromError(message);
            }
        }

        /// <summary>
        /// Converts an ADB reply to a string.
        /// </summary>
        /// <param name="reply">A <see cref="byte"/> array that represents the ADB reply.</param>
        /// <returns>A <see cref="string"/> that represents the ADB reply.</returns>
        protected string ReplyToString(byte[] reply)
        {
            string result;
            try
            {
                result = AdbClient.Encoding.GetString(reply);
            }
            catch (DecoderFallbackException e)
            {
                logger.LogError(e, e.Message);
                result = string.Empty;
            }
            return result;
        }

#if HAS_BUFFERS
        /// <summary>
        /// Converts an ADB reply to a string.
        /// </summary>
        /// <param name="reply">A <see cref="byte"/> array that represents the ADB reply.</param>
        /// <returns>A <see cref="string"/> that represents the ADB reply.</returns>
        protected string ReplyToString(ReadOnlySpan<byte> reply)
        {
            string result;
            try
            {
                result = AdbClient.Encoding.GetString(reply);
            }
            catch (DecoderFallbackException e)
            {
                logger.LogError(e, e.Message);
                result = string.Empty;
            }
            return result;
        }
#endif

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="AdbSocket"/> class.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Socket.Dispose();
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
            new StringBuilder(nameof(AdbSocket))
                .Append(" { ")
                .Append(nameof(Socket))
                .Append(" = ")
                .Append(Socket)
                .Append(" }")
                .ToString();

        /// <inheritdoc/>
        public void Close() => Socket.Dispose();

        /// <inheritdoc/>
        public virtual AdbSocket Clone() =>
            Socket is ICloneable<ITcpSocket> cloneable
                ? new AdbSocket(cloneable.Clone(), logger)
                : throw new NotSupportedException($"{Socket.GetType()} does not support cloning.");

        /// <inheritdoc/>
        object ICloneable.Clone() => Clone();

        /// <summary>
        /// Creates a new <see cref="TcpSocket"/> instance based on the specified <see cref="EndPoint"/>.
        /// </summary>
        /// <param name="endPoint">The <see cref="EndPoint"/> at which the Android Debug Bridge is listening for clients.</param>
        /// <returns>The new <see cref="TcpSocket"/> instance.</returns>
        private static TcpSocket CreateTcpSocket(EndPoint endPoint)
        {
            TcpSocket socket = new();
            socket.Connect(endPoint);
            socket.ReceiveBufferSize = ReceiveBufferSize;
            return socket;
        }
    }
}
