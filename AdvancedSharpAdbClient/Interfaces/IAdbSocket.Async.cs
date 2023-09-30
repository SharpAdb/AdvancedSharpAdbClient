#if HAS_TASK
// <copyright file="IAdbSocket.Async.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace AdvancedSharpAdbClient
{
    public partial interface IAdbSocket
    {
#if NET6_0_OR_GREATER
        /// <summary>
        /// Reconnects the <see cref="IAdbSocket"/> to the same endpoint it was initially connected to.
        /// Use this when the socket was disconnected by adb and you have restarted adb.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous task.</param>
        /// <returns>A <see cref="ValueTask"/> that represents the asynchronous operation.</returns>
        ValueTask ReconnectAsync(CancellationToken cancellationToken);
#endif

        /// <summary>
        /// Sends the specified number of bytes of data to a <see cref="IAdbSocket"/>,
        /// </summary>
        /// <param name="data">A <see cref="byte"/> array that acts as a buffer, containing the data to send.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <param name="length">The number of bytes to send.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task SendAsync(byte[] data, int length, CancellationToken cancellationToken);

        /// <summary>
        /// Sends the specified number of bytes of data to a <see cref="IAdbSocket"/>,
        /// </summary>
        /// <param name="data">A <see cref="byte"/> array that acts as a buffer, containing the data to send.</param>
        /// <param name="offset">The index of the first byte in the array to send.</param>
        /// <param name="length">The number of bytes to send.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task SendAsync(byte[] data, int offset, int length, CancellationToken cancellationToken);

        /// <summary>
        /// Sends a sync request to the device.
        /// </summary>
        /// <param name="command" >The command to send.</param>
        /// <param name="path">The path of the file on which the command should operate.</param>
        /// <param name="permissions">If the command is a <see cref="SyncCommand.SEND"/> command, the permissions to assign to the newly created file.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task SendSyncRequestAsync(SyncCommand command, string path, int permissions, CancellationToken cancellationToken);

        /// <summary>
        /// Sends a sync request to the device.
        /// </summary>
        /// <param name="command">The command to send.</param>
        /// <param name="path">The path of the file on which the command should operate.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task SendSyncRequestAsync(SyncCommand command, string path, CancellationToken cancellationToken);

        /// <summary>
        /// Sends a sync request to the device.
        /// </summary>
        /// <param name="command">The command to send.</param>
        /// <param name="length">The length of the data packet that follows.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task SendSyncRequestAsync(SyncCommand command, int length, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously sends a request to the Android Debug Bridge.To read the response, call
        /// <see cref="ReadAdbResponseAsync(CancellationToken)"/>.
        /// </summary>
        /// <param name="request">The request to send.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task SendAdbRequestAsync(string request, CancellationToken cancellationToken);

        /// <summary>
        /// Receives data from a <see cref="IAdbSocket"/> into a receive buffer.
        /// </summary>
        /// <param name="data">An array of type <see cref="byte"/> that is the storage location for the received data.</param>
        /// <param name="length">The number of bytes to receive.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <remarks>Cancelling the task will also close the socket.</remarks>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. The result value of the task contains the number of bytes received.</returns>
        Task<int> ReadAsync(byte[] data, int length, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously reads a <see cref="string"/> from an <see cref="IAdbSocket"/> instance.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. The return value of the task is the<see cref="string"/> received from the <see cref = "IAdbSocket"/>.</returns>
        Task<string> ReadStringAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Reads a <see cref="string"/> from an <see cref="IAdbSocket"/> instance when
        /// the connection is in sync mode.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. The return value of the task is the <see cref="string"/> received from the <see cref = "IAdbSocket"/>.</returns>
        Task<string> ReadSyncStringAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Reads the response to a sync command.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. The return value of the task is the response that was sent by the device.</returns>
        Task<SyncCommand> ReadSyncResponseAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously receives an <see cref="AdbResponse"/> message, and throws an error
        /// if the message does not indicate success.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>A <see cref="AdbResponse"/> object that represents the response from the Android Debug Bridge.</returns>
        Task<AdbResponse> ReadAdbResponseAsync(CancellationToken cancellationToken);

#if HAS_BUFFERS
        /// <summary>
        /// Sends the specified number of bytes of data to a <see cref="IAdbSocket"/>,
        /// </summary>
        /// <param name="data">A <see cref="byte"/> array that acts as a buffer, containing the data to send.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>A <see cref="ValueTask"/> that represents the asynchronous operation.</returns>
        public ValueTask SendAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default);

        /// <summary>
        /// Receives data from a <see cref="IAdbSocket"/> into a receive buffer.
        /// </summary>
        /// <param name="data">An array of type Byte that is the storage location for the received data.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <remarks>Cancelling the task will also close the socket.</remarks>
        /// <returns>A <see cref="ValueTask"/> that represents the asynchronous operation. The result value of the task contains the number of bytes received.</returns>
        public ValueTask<int> ReadAsync(Memory<byte> data, CancellationToken cancellationToken);
#else
        /// <summary>
        /// Sends the specified number of bytes of data to a <see cref="IAdbSocket"/>,
        /// </summary>
        /// <param name="data">A <see cref="byte"/> array that acts as a buffer, containing the data to send.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task SendAsync(byte[] data, CancellationToken cancellationToken);
        
        /// <summary>
        /// Reads a <see cref="string"/> from an <see cref="IAdbSocket"/> instance when
        /// the connection is in sync mode.
        /// </summary>
        /// <param name="data" >The buffer to store the read data into.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. The result value of the task contains the number of bytes received.</returns>
        Task<int> ReadAsync(byte[] data, CancellationToken cancellationToken);
#endif

        /// <summary>
        /// Ask to switch the connection to the device/emulator identified by
        /// <paramref name="device"/>. After this request, every client request will
        /// be sent directly to the adbd daemon running on the device.
        /// </summary>
        /// <param name="device">The device to which to connect.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <remarks>If <paramref name="device"/> is <see langword="null"/>, this method does nothing.</remarks>
        Task SetDeviceAsync(DeviceData device, CancellationToken cancellationToken);
    }
}
#endif