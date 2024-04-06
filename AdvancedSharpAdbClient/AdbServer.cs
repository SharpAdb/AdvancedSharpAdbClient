// <copyright file="AdbServer.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace AdvancedSharpAdbClient
{
    /// <summary>
    /// <para>Provides methods for interacting with the adb server. The adb server must be running for
    /// the rest of the <c>Managed.Adb</c> library to work.</para>
    /// <para>The adb server is a background process that runs on the host machine.
    /// Its purpose if to sense the USB ports to know when devices are attached/removed,
    /// as well as when emulator instances start/stop. The ADB server is really one
    /// giant multiplexing loop whose purpose is to orchestrate the exchange of data
    /// between clients and devices.</para>
    /// </summary>
    [DebuggerDisplay($"{nameof(AdbServer)} \\{{ {nameof(EndPoint)} = {{{nameof(EndPoint)}}}, {nameof(CachedAdbPath)} = {{{nameof(CachedAdbPath)}}} }}")]
    public partial class AdbServer : IAdbServer, ICloneable<AdbServer>, ICloneable
    {
        /// <summary>
        /// The minimum version of <c>adb.exe</c> that is supported by this library.
        /// </summary>
        public static readonly Version RequiredAdbVersion = new(1, 0, 20);

        /// <summary>
        /// The error code that is returned by the <see cref="SocketException"/> when the connection is refused.
        /// </summary>
        /// <remarks>No connection could be made because the target computer actively refused it.This usually
        /// results from trying to connect to a service that is inactive on the foreign host—that is,
        /// one with no server application running. <seealso href="https://msdn.microsoft.com/en-us/library/ms740668.aspx"/></remarks>
        public const int ConnectionRefused = 10061;

        /// <summary>
        /// The error code that is returned by the <see cref="SocketException"/> when the connection was reset by the peer.
        /// </summary>
        /// <remarks>An existing connection was forcibly closed by the remote host. This normally results if the peer application on the
        /// remote host is suddenly stopped, the host is rebooted, the host or remote network interface is disabled, or the remote
        /// host uses a hard close. This error may also result if a connection was broken due to keep-alive activity detecting
        /// a failure while one or more operations are in progress. <seealso href="https://msdn.microsoft.com/en-us/library/ms740668.aspx"/></remarks>
        public const int ConnectionReset = 10054;

        /// <summary>
        /// The <see cref="Func{EndPoint, IAdbSocket}"/> to create <see cref="IAdbSocket"/>.
        /// </summary>
        protected readonly Func<EndPoint, IAdbSocket> AdbSocketFactory;

        /// <summary>
        /// Gets or sets a function that returns a new instance of a class that implements the
        /// <see cref="IAdbCommandLineClient"/> interface, that can be used to interact with the
        /// <c>adb.exe</c> command line client.
        /// </summary>
        protected readonly Func<string, IAdbCommandLineClient> AdbCommandLineClientFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdbServer"/> class.
        /// </summary>
        public AdbServer()
            : this(AdbClient.AdbServerEndPoint, Factories.AdbSocketFactory, Factories.AdbCommandLineClientFactory)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdbServer"/> class.
        /// </summary>
        /// <param name="adbClient">A connection to an adb server.</param>
        public AdbServer(IAdbClient adbClient)
            : this(adbClient.EndPoint, Factories.AdbSocketFactory, Factories.AdbCommandLineClientFactory)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdbServer"/> class.
        /// </summary>
        /// <param name="endPoint">The <see cref="System.Net.EndPoint"/> at which the adb server is listening.</param>
        public AdbServer(EndPoint endPoint)
            : this(endPoint, Factories.AdbSocketFactory, Factories.AdbCommandLineClientFactory)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdbServer"/> class.
        /// </summary>
        /// <param name="host">The host address at which the adb server is listening.</param>
        /// <param name="port">The port at which the adb server is listening.</param>
        public AdbServer(string host, int port)
            : this(Extensions.CreateDnsEndPoint(host, port), Factories.AdbSocketFactory, Factories.AdbCommandLineClientFactory)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdbServer"/> class.
        /// </summary>
        /// <param name="adbClient">A connection to an adb server.</param>
        /// <param name="adbSocketFactory">The <see cref="Func{EndPoint, IAdbSocket}"/> to create <see cref="IAdbSocket"/>.</param>
        /// <param name="adbCommandLineClientFactory">The <see cref="Func{String, IAdbCommandLineClient}"/> to create <see cref="IAdbCommandLineClient"/>.</param>
        public AdbServer(IAdbClient adbClient, Func<EndPoint, IAdbSocket> adbSocketFactory, Func<string, IAdbCommandLineClient> adbCommandLineClientFactory)
            : this(adbClient.EndPoint, adbSocketFactory, adbCommandLineClientFactory)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdbServer"/> class.
        /// </summary>
        /// <param name="endPoint">The <see cref="System.Net.EndPoint"/> at which the adb server is listening.</param>
        /// <param name="adbSocketFactory">The <see cref="Func{EndPoint, IAdbSocket}"/> to create <see cref="IAdbSocket"/>.</param>
        /// <param name="adbCommandLineClientFactory">The <see cref="Func{String, IAdbCommandLineClient}"/> to create <see cref="IAdbCommandLineClient"/>.</param>
        public AdbServer(EndPoint endPoint, Func<EndPoint, IAdbSocket> adbSocketFactory, Func<string, IAdbCommandLineClient> adbCommandLineClientFactory)
        {
            ExceptionExtensions.ThrowIfNull(endPoint);

            if (endPoint is not (IPEndPoint or DnsEndPoint))
            {
                throw new NotSupportedException("Only TCP endpoints are supported");
            }

            EndPoint = endPoint;
            AdbSocketFactory = adbSocketFactory ?? throw new ArgumentNullException(nameof(adbSocketFactory));
            AdbCommandLineClientFactory = adbCommandLineClientFactory ?? throw new ArgumentNullException(nameof(adbCommandLineClientFactory));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdbServer"/> class.
        /// </summary>
        /// <param name="host">The host address at which the adb server is listening.</param>
        /// <param name="port">The port at which the adb server is listening.</param>
        /// <param name="adbSocketFactory">The <see cref="Func{EndPoint, IAdbSocket}"/> to create <see cref="IAdbSocket"/>.</param>
        /// <param name="adbCommandLineClientFactory">The <see cref="Func{String, IAdbCommandLineClient}"/> to create <see cref="IAdbCommandLineClient"/>.</param>
        public AdbServer(string host, int port, Func<EndPoint, IAdbSocket> adbSocketFactory, Func<string, IAdbCommandLineClient> adbCommandLineClientFactory)
            : this(Extensions.CreateDnsEndPoint(host, port), adbSocketFactory, adbCommandLineClientFactory)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdbServer"/> class.
        /// </summary>
        /// <param name="adbSocketFactory">The <see cref="Func{EndPoint, IAdbSocket}"/> to create <see cref="IAdbSocket"/>.</param>
        /// <param name="adbCommandLineClientFactory">The <see cref="Func{String, IAdbCommandLineClient}"/> to create <see cref="IAdbCommandLineClient"/>.</param>
        public AdbServer(Func<EndPoint, IAdbSocket> adbSocketFactory, Func<string, IAdbCommandLineClient> adbCommandLineClientFactory)
            : this(AdbClient.AdbServerEndPoint, adbSocketFactory, adbCommandLineClientFactory)
        {
        }

        /// <summary>
        /// Gets or sets the default instance of the <see cref="IAdbServer"/> interface.
        /// </summary>
        public static IAdbServer Instance { get; set; } = new AdbServer();

        /// <summary>
        /// <see langword="true"/> if is starting adb server; otherwise, <see langword="false"/>.
        /// </summary>
        protected static bool IsStarting { get; set; } = false;

        /// <summary>
        /// The path to the adb server. Cached from calls to <see cref="StartServer(string, bool)"/>. Used when restarting
        /// the server to figure out where adb is located.
        /// </summary>
        protected string? CachedAdbPath { get; set; }

        /// <summary>
        /// Gets the <see cref="System.Net.EndPoint"/> at which the adb server is listening.
        /// </summary>
        public EndPoint EndPoint { get; init; }

        /// <inheritdoc/>
        public StartServerResult StartServer(string adbPath, bool restartServerIfNewer = false)
        {
            if (IsStarting) { return StartServerResult.Starting; }
            try
            {
                AdbServerStatus serverStatus = GetStatus();
                Version? commandLineVersion = null;

                IAdbCommandLineClient commandLineClient = AdbCommandLineClientFactory(adbPath);

                if (commandLineClient.CheckAdbFileExists(adbPath))
                {
                    CachedAdbPath = adbPath;
                    commandLineVersion = commandLineClient.GetVersion().AdbVersion;
                }

                // If the server is running, and no adb path is provided, check if we have the minimum version
                if (adbPath == null)
                {
                    return !serverStatus.IsRunning
                        ? throw new AdbException("The adb server is not running, but no valid path to the adb.exe executable was provided. The adb server cannot be started.")
                        : serverStatus.Version >= RequiredAdbVersion
                            ? StartServerResult.AlreadyRunning
                            : throw new AdbException($"The adb daemon is running an outdated version ${commandLineVersion}, but not valid path to the adb.exe executable was provided. A more recent version of the adb server cannot be started.");
                }

                if (serverStatus.IsRunning)
                {
                    if (serverStatus.Version < RequiredAdbVersion
                        || (restartServerIfNewer && serverStatus.Version < commandLineVersion))
                    {
                        StopServer();
                        commandLineClient.StartServer(Timeout.Infinite);
                        return StartServerResult.RestartedOutdatedDaemon;
                    }
                    else
                    {
                        return StartServerResult.AlreadyRunning;
                    }
                }
                else
                {
                    commandLineClient.StartServer(Timeout.Infinite);
                    return StartServerResult.Started;
                }
            }
            finally
            {
                IsStarting = false;
            }
        }

        /// <inheritdoc/>
        public StartServerResult RestartServer() => StartServer(CachedAdbPath!, true);

        /// <inheritdoc/>
        public StartServerResult RestartServer(string adbPath) =>
            StringExtensions.IsNullOrWhiteSpace(adbPath) ? RestartServer() : StartServer(adbPath, true);

        /// <inheritdoc/>
        public void StopServer()
        {
            using IAdbSocket socket = CreateAdbSocket();
            socket.SendAdbRequest("host:kill");

            // The host will immediately close the connection after the kill
            // command has been sent; no need to read the response.
        }

        /// <inheritdoc/>
        public AdbServerStatus GetStatus()
        {
            // Try to connect to a running instance of the adb server
            try
            {
                using IAdbSocket socket = CreateAdbSocket();
                socket.SendAdbRequest("host:version");
                AdbResponse response = socket.ReadAdbResponse();
                string version = socket.ReadString();

                int versionCode = int.Parse(version, NumberStyles.HexNumber);
                return new AdbServerStatus(true, new Version(1, 0, versionCode));
            }
            catch (SocketException ex)
            {
                if (ex.SocketErrorCode == SocketError.ConnectionRefused)
                {
                    return new AdbServerStatus(false, null);
                }
                else
                {
                    // An unexpected exception occurred; re-throw the exception
                    throw;
                }
            }
        }

        /// <summary>
        /// Creates a new <see cref="AdbClient"/> object with the specified <see cref="EndPoint"/>.
        /// </summary>
        /// <returns>A new <see cref="AdbClient"/> object with the specified <see cref="EndPoint"/>.</returns>
        public IAdbSocket CreateAdbSocket() => AdbSocketFactory(EndPoint);

        /// <inheritdoc/>
        public override string ToString() => $"The {nameof(AdbServer)} communicate with adb at {EndPoint}";

        /// <summary>
        /// Creates a new <see cref="AdbServer"/> object that is a copy of the current instance with new <see cref="EndPoint"/>.
        /// </summary>
        /// <param name="endPoint">The new <see cref="EndPoint"/> to use.</param>
        /// <returns>A new <see cref="AdbServer"/> object that is a copy of this instance with new <see cref="EndPoint"/>.</returns>
        public virtual AdbServer Clone(EndPoint endPoint) => new(endPoint, AdbSocketFactory, AdbCommandLineClientFactory);

        /// <inheritdoc/>
        public AdbServer Clone() => Clone(EndPoint);

        /// <inheritdoc/>
        object ICloneable.Clone() => Clone(EndPoint);
    }
}
