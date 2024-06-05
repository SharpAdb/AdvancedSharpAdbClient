// <copyright file="IAdbSocket.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.Net.Sockets;

namespace AdvancedSharpAdbClient
{
    /// <summary>
    /// Provides a common interface for any class that acts as a client for the
    /// Android Debug Bridge.
    /// </summary>
    public partial interface IAdbSocket : IDisposable
    {
        /// <summary>
        /// Gets a value indicating whether the <see cref="AdbSocket"/> is connected to a remote
        /// host as of the latest send or receive operation.
        /// </summary>
        bool Connected { get; }

        /// <summary>
        /// Reconnects the <see cref="IAdbSocket"/> to the same endpoint it was initially connected to.
        /// Use this when the socket was disconnected by adb and you have restarted adb.
        /// </summary>
        /// <param name="isForce">Force reconnect whatever the socket is connected or not.</param>
        void Reconnect(bool isForce);

        /// <summary>
        /// Sends the specified number of bytes of data to a <see cref="IAdbSocket"/>,
        /// </summary>
        /// <param name="data">A <see cref="byte"/> array that acts as a buffer, containing the data to send.</param>
        void Send(byte[] data);

        /// <summary>
        /// Sends the specified number of bytes of data to a <see cref="IAdbSocket"/>,
        /// </summary>
        /// <param name="data">A <see cref="byte"/> array that acts as a buffer, containing the data to send.</param>
        /// <param name="length">The number of bytes to send.</param>
        void Send(byte[] data, int length);

        /// <summary>
        /// Sends the specified number of bytes of data to a <see cref="IAdbSocket"/>,
        /// </summary>
        /// <param name="data">A <see cref="byte"/> array that acts as a buffer, containing the data to send.</param>
        /// <param name="offset">The index of the first byte in the array to send.</param>
        /// <param name="length">The number of bytes to send.</param>
        void Send(byte[] data, int offset, int length);

        /// <summary>
        /// Sends a sync request to the device.
        /// </summary>
        /// <param name="command" >The command to send.</param>
        /// <param name="path">The path of the file on which the command should operate.</param>
        /// <param name="permissions">If the command is a <see cref="SyncCommand.SEND"/> command, the permissions to assign to the newly created file.</param>
        void SendSyncRequest(SyncCommand command, string path, UnixFileStatus permissions);

        /// <summary>
        /// Sends a sync request to the device.
        /// </summary>
        /// <param name="command">The command to send.</param>
        /// <param name="path">The path of the file on which the command should operate.</param>
        void SendSyncRequest(SyncCommand command, string path);

        /// <summary>
        /// Sends a sync request to the device.
        /// </summary>
        /// <param name="command">The command to send.</param>
        /// <param name="length">The length of the data packet that follows.</param>
        void SendSyncRequest(SyncCommand command, int length);

        /// <summary>
        /// Sends a request to the Android Debug Bridge.To read the response, call
        /// <see cref="ReadAdbResponse()"/>.
        /// </summary>
        /// <param name="request">The request to send.</param>
        void SendAdbRequest(string request);

        /// <summary>
        /// Reads from the socket until the array is filled, or no more data is coming(because
        /// the socket closed or the timeout expired).
        /// </summary>
        /// <param name="data" >The buffer to store the read data into.</param>
        /// <returns>The total number of bytes read.</returns>
        /// <remarks>This uses the default time out value.</remarks>
        int Read(byte[] data);

        /// <summary>
        /// Reads from the socket until the array is filled, the optional<paramref name= "length"/>
        /// is reached, or no more data is coming (because the socket closed or the
        /// timeout expired).
        /// </summary>
        /// <param name="data">The buffer to store the read data into.</param>
        /// <param name="length">The length to read or -1 to fill the data buffer completely</param>
        /// <returns>The total number of bytes read.</returns>
        /// <exception cref="AdbException">EOF or No Data to read: exception.Message</exception>
        int Read(byte[] data, int length);

        /// <summary>
        /// Reads from the socket until the array is filled, the optional<paramref name= "length"/>
        /// is reached, or no more data is coming (because the socket closed or the
        /// timeout expired).
        /// </summary>
        /// <param name="data">The buffer to store the read data into.</param>
        /// <param name="offset">The position to store the received data.</param>
        /// <param name="length">The length to read or -1 to fill the data buffer completely</param>
        /// <returns>The total number of bytes read.</returns>
        /// <exception cref="AdbException">EOF or No Data to read: exception.Message</exception>
        int Read(byte[] data, int offset, int length);

        /// <summary>
        /// Reads a <see cref="string"/> from an <see cref="IAdbSocket"/> instance.
        /// </summary>
        /// <returns>The <see cref="string"/> received from the <see cref = "IAdbSocket"/>.</returns>
        string ReadString();

        /// <summary>
        /// Reads a <see cref="string"/> from an <see cref="IAdbSocket"/> instance when
        /// the connection is in sync mode.
        /// </summary>
        /// <returns>The <see cref="string"/> received from the <see cref = "IAdbSocket"/>.</returns>
        string ReadSyncString();

        /// <summary>
        /// Reads the response to a sync command.
        /// </summary>
        /// <returns>The response that was sent by the device.</returns>
        SyncCommand ReadSyncResponse();

        /// <summary>
        /// Receives an <see cref="AdbResponse"/> message, and throws an error
        /// if the message does not indicate success.
        /// </summary>
        /// <returns>A <see cref="AdbResponse"/> object that represents the response from the Android Debug Bridge.</returns>
        AdbResponse ReadAdbResponse();

#if HAS_BUFFERS
        /// <summary>
        /// Sends the specified number of bytes of data to a <see cref="IAdbSocket"/>,
        /// </summary>
        /// <param name="data">A span of bytes that acts as a buffer, containing the data to send.</param>
        void Send(ReadOnlySpan<byte> data)
#if COMP_NETSTANDARD2_1
            => Send(data.ToArray())
#endif
            ;

        /// <summary>
        /// Reads from the socket until the array is filled, or no more data is coming(because
        /// the socket closed or the timeout expired).
        /// </summary>
        /// <param name="data" >A span of bytes to store the read data into.</param>
        /// <returns>The total number of bytes read.</returns>
        /// <remarks>This uses the default time out value.</remarks>
        int Read(Span<byte> data)
#if COMP_NETSTANDARD2_1
        {
            byte[] bytes = new byte[data.Length];
            int length = Read(bytes);
            for (int i = 0; i < length; i++)
            {
                data[i] = bytes[i];
            }
            return length;
        }
#else
            ;
#endif
#endif

        /// <summary>
        /// Gets a <see cref="Stream"/> that can be used to send and receive shell output to and
        /// from the device.
        /// </summary>
        /// <returns>
        /// A <see cref="Stream"/> that can be used to send and receive shell output to and
        /// from the device.
        /// </returns>
        Stream GetShellStream();

        /// <summary>
        /// Ask to switch the connection to the device/emulator identified by
        /// <paramref name="device"/>. After this request, every client request will
        /// be sent directly to the adbd daemon running on the device.
        /// </summary>
        /// <param name="device">The device to which to connect.</param>
        /// <remarks>If <paramref name="device"/> is <see langword="null"/>, this method does nothing.</remarks>
        void SetDevice(DeviceData device);

        /// <summary>
        /// Closes the <see cref="Socket"/> connection and releases all associated resources.
        /// </summary>
        void Close();
    }
}
