// <copyright file="AdbClientExtensions.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

using AdvancedSharpAdbClient.Exceptions;
using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AdvancedSharpAdbClient
{
    /// <summary>
    /// Provides extension methods for the <see cref="IAdbClient"/> interface. Provides overloads for commonly used funtions.
    /// </summary>
    public static class AdbClientExtensions
    {
        /// <summary>
        /// Creates a port forwarding between a local and a remote port.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="device">The device to which to forward the connections.</param>
        /// <param name="localPort">The local port to forward.</param>
        /// <param name="remotePort">The remote port to forward to</param>
        /// <exception cref="AdbException">failed to submit the forward command. or Device rejected command:  + resp.Message</exception>
        public static int CreateForward(this IAdbClient client, DeviceData device, int localPort, int remotePort)
        {
            return client.CreateForward(device, $"tcp:{localPort}", $"tcp:{remotePort}", true);
        }

        /// <summary>
        /// Forwards a remote Unix socket to a local TCP socket.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="device">The device to which to forward the connections.</param>
        /// <param name="localPort">The local port to forward.</param>
        /// <param name="remoteSocket">The remote Unix socket.</param>
        /// <exception cref="AdbException">The client failed to submit the forward command.</exception>
        /// <exception cref="AdbException">The device rejected command. The error message will include the error message provided by the device.</exception>
        public static int CreateForward(this IAdbClient client, DeviceData device, int localPort, string remoteSocket)
        {
            return client.CreateForward(device, $"tcp:{localPort}", $"local:{remoteSocket}", true);
        }

        /// <summary>
        /// Executes a shell command on the remote device
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="command">The command to execute</param>
        /// <param name="device">The device to execute on</param>
        /// <param name="rcvr">The shell output receiver</param>
        public static void ExecuteRemoteCommand(this IAdbClient client, string command, DeviceData device, IShellOutputReceiver rcvr)
        {
            ExecuteRemoteCommand(client, command, device, rcvr, AdbClient.Encoding);
        }

        /// <summary>
        /// Executes a shell command on the remote device
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="command">The command to execute</param>
        /// <param name="device">The device to execute on</param>
        /// <param name="rcvr">The shell output receiver</param>
        /// <param name="encoding">The encoding to use.</param>
        public static void ExecuteRemoteCommand(this IAdbClient client, string command, DeviceData device, IShellOutputReceiver rcvr, Encoding encoding)
        {
            try
            {
                client.ExecuteRemoteCommandAsync(command, device, rcvr, encoding, CancellationToken.None).Wait();
            }
            catch (AggregateException ex)
            {
                if (ex.InnerExceptions.Count == 1)
                {
                    throw ex.InnerException;
                }
                else
                {
                    throw ex;
                }
            }
        }

        /// <summary>
        /// Reboots the specified adb socket address.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="device">The device.</param>
        public static void Reboot(this IAdbClient client, DeviceData device)
        {
            client.Reboot(string.Empty, device);
        }

        /// <summary>
        /// Connect to a device via TCP/IP.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="address">The IP address of the remote device.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>An <see cref="Task"/> which return the results from adb.</returns>
        public static Task<string> Connect(this IAdbClient client, IPAddress address, CancellationToken cancellationToken = default)
        {
            if (address == null)
            {
                throw new ArgumentNullException(nameof(address));
            }

            return client.Connect(new IPEndPoint(address, AdbClient.DefaultPort), cancellationToken);
        }

        /// <summary>
        /// Connect to a device via TCP/IP.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="host">The host address of the remote device.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>An <see cref="Task"/> which return the results from adb.</returns>
        public static Task<string> Connect(this IAdbClient client, string host, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(host))
            {
                throw new ArgumentNullException(nameof(host));
            }

            string[] values = host.Split(':');

            if (values.Length <= 0)
            {
                throw new ArgumentNullException(nameof(host));
            }

            return client.Connect(new DnsEndPoint(values[0], values.Length == 1 ? AdbClient.DefaultPort : int.Parse(values[1])), cancellationToken);
        }

        /// <summary>
        /// Connect to a device via TCP/IP.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="endpoint">The IP endpoint at which the <c>adb</c> server on the device is running.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>An <see cref="Task"/> which return the results from adb.</returns>
        public static Task<string> Connect(this IAdbClient client, IPEndPoint endpoint, CancellationToken cancellationToken = default)
        {
            if (endpoint == null)
            {
                throw new ArgumentNullException(nameof(endpoint));
            }

            return client.Connect(new DnsEndPoint(endpoint.Address.ToString(), endpoint.Port), cancellationToken);
        }
    }
}
