// <copyright file="AdbSocket.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using AdvancedSharpAdbClient.Exceptions;
using AdvancedSharpAdbClient.Logs;
using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

#if HAS_LOGGER
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
#endif

namespace AdvancedSharpAdbClient
{

    /// <summary>
    /// <para>
    /// Implements a client for the Android Debug Bridge client-server protocol. Using the client, you
    /// can send messages to and receive messages from the Android Debug Bridge.
    /// </para>
    /// <para>
    /// The <see cref="AdbSocket"/> class implements the raw messaging protocol; that is,
    /// sending and receiving messages. For interacting with the services the Android Debug
    /// Bridge exposes, use the <see cref="AdbClient"/>.
    /// </para>
    /// <para>
    /// For more information about the protocol that is implemented here, see chapter
    /// II Protocol Details, section 1. Client &lt;-&gt;Server protocol at
    /// <see href="https://android.googlesource.com/platform/system/core/+/master/adb/OVERVIEW.TXT"/>.
    /// </para>
    /// </summary>
    public class AdbSocket : IAdbSocket, IDisposable
    {
        /// <summary>
        /// The underlying TCP socket that manages the connection with the ADB server.
        /// </summary>
        private readonly ITcpSocket socket;

#if HAS_LOGGER
        /// <summary>
        /// The logger to use when logging messages.
        /// </summary>
        private readonly ILogger<AdbSocket> logger;
#endif

#if !HAS_LOGGER
#pragma warning disable CS1572 // XML 注释中有 param 标记，但是没有该名称的参数
#endif
        /// <summary>
        /// Initializes a new instance of the <see cref="AdbSocket"/> class.
        /// </summary>
        /// <param name="endPoint">The <see cref="EndPoint"/> at which the Android Debug Bridge is listening for clients.</param>
        /// <param name="logger">The logger to use when logging.</param>
        public AdbSocket(EndPoint endPoint
#if HAS_LOGGER
            , ILogger<AdbSocket> logger = null
#endif
            )
        {
            socket = new TcpSocket();
            socket.Connect(endPoint);
            socket.ReceiveBufferSize = ReceiveBufferSize;
#if HAS_LOGGER
            this.logger = logger ?? NullLogger<AdbSocket>.Instance;
#endif
        }
#if !HAS_LOGGER
#pragma warning restore CS1572 // XML 注释中有 param 标记，但是没有该名称的参数
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="AdbSocket"/> class.
        /// </summary>
        /// <param name="socket">The <see cref="ITcpSocket"/> at which the Android Debug Bridge is listening for clients.</param>
        public AdbSocket(ITcpSocket socket)
        {
            this.socket = socket;
#if HAS_LOGGER
            logger ??= NullLogger<AdbSocket>.Instance;
#endif
        }

        /// <summary>
        /// Gets or sets the size of the receive buffer
        /// </summary>
        public static int ReceiveBufferSize { get; set; } = 40960;

        /// <summary>
        /// Gets or sets the size of the write buffer.
        /// </summary>
        public static int WriteBufferSize { get; set; } = 1024;

        /// <inheritdoc/>
        public bool Connected => socket.Connected;

        /// <summary>
        /// Determines whether the specified reply is okay.
        /// </summary>
        /// <param name="reply">The reply.</param>
        /// <returns><see langword="true"/> if the specified reply is okay; otherwise, <see langword="false"/>.</returns>
        public static bool IsOkay(byte[] reply) => AdbClient.Encoding.GetString(reply).Equals("OKAY");

        /// <inheritdoc/>
        public virtual void Reconnect() => socket.Reconnect();

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="AdbSocket"/> class.
        /// </summary>
        public virtual void Dispose() => socket.Dispose();

        /// <inheritdoc/>
        public virtual int Read(byte[] data) => Read(data, data.Length);

        /// <inheritdoc/>
        public virtual Task ReadAsync(byte[] data, CancellationToken cancellationToken = default) =>
            ReadAsync(data, data.Length, cancellationToken);

        /// <inheritdoc/>
        public virtual void SendSyncRequest(SyncCommand command, string path, int permissions) =>
            SendSyncRequest(command, $"{path},{permissions}");

        /// <inheritdoc/>
        public virtual void SendSyncRequest(SyncCommand command, string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            byte[] pathBytes = AdbClient.Encoding.GetBytes(path);
            SendSyncRequest(command, pathBytes.Length);
            _ = Write(pathBytes);
        }

        /// <inheritdoc/>
        public virtual void SendSyncRequest(SyncCommand command, int length)
        {
            // The message structure is:
            // First four bytes: command
            // Next four bytes: length of the path
            // Final bytes: path
            byte[] commandBytes = SyncCommandConverter.GetBytes(command);

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
        public virtual SyncCommand ReadSyncResponse()
        {
            byte[] data = new byte[4];
            _ = Read(data);

            return SyncCommandConverter.GetCommand(data);
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
                return null;
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
            _ = Read(reply);

            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(reply);
            }

            int len = BitConverter.ToInt32(reply, 0);

            // And get the string
            reply = new byte[len];
            _ = Read(reply);

            string value = AdbClient.Encoding.GetString(reply);
            return value;
        }

        /// <inheritdoc/>
        public virtual async Task<string> ReadStringAsync(CancellationToken cancellationToken = default)
        {
            // The first 4 bytes contain the length of the string
            byte[] reply = new byte[4];
            await ReadAsync(reply, cancellationToken).ConfigureAwait(false);

            // Convert the bytes to a hex string
            string lenHex = AdbClient.Encoding.GetString(reply);
            int len = int.Parse(lenHex, NumberStyles.HexNumber);

            // And get the string
            reply = new byte[len];
            await ReadAsync(reply, cancellationToken).ConfigureAwait(false);

            string value = AdbClient.Encoding.GetString(reply);
            return value;
        }

        /// <inheritdoc/>
        public virtual AdbResponse ReadAdbResponse()
        {
            AdbResponse response = ReadAdbResponseInner();

            if (!response.IOSuccess || !response.Okay)
            {
                socket.Dispose();
                throw new AdbException($"An error occurred while reading a response from ADB: {response.Message}", response);
            }

            return response;
        }

        /// <inheritdoc/>
        public virtual void SendAdbRequest(string request)
        {
            byte[] data = AdbClient.FormAdbRequest(request);

            if (!Write(data))
            {
                throw new IOException($"Failed sending the request '{request}' to ADB");
            }
        }

        /// <inheritdoc/>
        public virtual void Send(byte[] data, int length) => Send(data, 0, length);

        /// <inheritdoc/>
        public virtual void Send(byte[] data, int offset, int length)
        {
            try
            {
                int count = socket.Send(data, 0, length != -1 ? length : data.Length, SocketFlags.None);
                if (count < 0)
                {
                    throw new AdbException("channel EOF");
                }
            }
            catch (SocketException sex)
            {
#if HAS_LOGGER
                logger.LogError(sex, sex.Message);
#endif
                throw sex;
            }
        }

        /// <inheritdoc/>
        public virtual async Task<int> ReadAsync(byte[] data, int length, CancellationToken cancellationToken = default)
        {
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (data.Length < length)
            {
                throw new ArgumentOutOfRangeException(nameof(data));
            }

            int count = -1;
            int totalRead = 0;

            while (count != 0 && totalRead < length)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    int left = length - totalRead;
                    int buflen = left < ReceiveBufferSize ? left : ReceiveBufferSize;

                    count = await socket.ReceiveAsync(data, totalRead, buflen, SocketFlags.None, cancellationToken).ConfigureAwait(false);

                    if (count < 0)
                    {
#if HAS_LOGGER
                        logger.LogError("read: channel EOF");
#endif
                        throw new AdbException("EOF");
                    }
                    else if (count == 0)
                    {
#if HAS_LOGGER
                        logger.LogInformation("DONE with Read");
#endif
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
        public virtual int Read(byte[] data, int length)
        {
            int expLen = length != -1 ? length : data.Length;
            int count = -1;
            int totalRead = 0;

            while (count != 0 && totalRead < expLen)
            {
                try
                {
                    int left = expLen - totalRead;
                    int buflen = left < ReceiveBufferSize ? left : ReceiveBufferSize;

                    byte[] buffer = new byte[buflen];
                    count = socket.Receive(buffer, buflen, SocketFlags.None);
                    if (count < 0)
                    {
#if HAS_LOGGER
                        logger.LogError("read: channel EOF");
#endif
                        throw new AdbException("EOF");
                    }
                    else if (count == 0)
                    {
#if HAS_LOGGER
                        logger.LogInformation("DONE with Read");
#endif
                    }
                    else
                    {
                        Array.Copy(buffer, 0, data, totalRead, count);
                        totalRead += count;
                    }
                }
                catch (SocketException sex)
                {
                    throw new AdbException(string.Format("No Data to read: {0}", sex.Message));
                }
            }

            return totalRead;
        }

        /// <inheritdoc/>
        public void SetDevice(DeviceData device)
        {
            // if the device is not null, then we first tell adb we're looking to talk
            // to a specific device
            if (device != null)
            {
                SendAdbRequest($"host:transport:{device.Serial}");

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
                        throw e;
                    }
                }
            }
        }

        /// <inheritdoc/>
        public Stream GetShellStream()
        {
            Stream stream = socket.GetStream();
            return new ShellStream(stream, closeStream: true);
        }

        /// <summary>
        /// Write until all data in "data" is written or the connection fails or times out.
        /// </summary>
        /// <param name="data">The data to send.</param>
        /// <returns>Returns <see langword="true"/> if all data was written; otherwise, <see langword="false"/>.</returns>
        /// <remarks>This uses the default time out value.</remarks>
        protected bool Write(byte[] data)
        {
            try
            {
                Send(data, -1);
            }
#if HAS_LOGGER
            catch (IOException e)
            {
                logger.LogError(e, e.Message);
#else
            catch (IOException)
            {
#endif
                return false;
            }

            return true;
        }

        /// <summary>
        /// Reads the response from ADB after a command.
        /// </summary>
        /// <returns>A <see cref="AdbResponse"/> that represents the response received from ADB.</returns>
        protected AdbResponse ReadAdbResponseInner()
        {
            AdbResponse resp = new();

            byte[] reply = new byte[4];
            Read(reply);

            resp.IOSuccess = true;

            resp.Okay = IsOkay(reply);

            if (!resp.Okay)
            {
                string message = ReadString();
                resp.Message = message;
#if HAS_LOGGER
                logger.LogError("Got reply '{0}', diag='{1}'", ReplyToString(reply), resp.Message);
#endif
            }

            return resp;
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
                result = Encoding.ASCII.GetString(reply);
            }
#if HAS_LOGGER
            catch (DecoderFallbackException e)
            {
                logger.LogError(e, e.Message);
#else
            catch (DecoderFallbackException)
            {
#endif
                result = string.Empty;
            }

            return result;
        }
    }
}
