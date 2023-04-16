using AdvancedSharpAdbClient.Exceptions;
using AdvancedSharpAdbClient.WinRT.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;

namespace AdvancedSharpAdbClient.WinRT
{
    /// <summary>
    /// <para>
    /// Provides methods for interacting with the adb server. The adb server must be running for
    /// the rest of the <c>Managed.Adb</c> library to work.
    /// </para>
    /// <para>
    /// The adb server is a background process that runs on the host machine.
    /// Its purpose if to sense the USB ports to know when devices are attached/removed,
    /// as well as when emulator instances start/stop. The ADB server is really one
    /// giant multiplexing loop whose purpose is to orchestrate the exchange of data
    /// between clients and devices.
    /// </para>
    /// </summary>
    public sealed class AdbServer
    {
        private AdvancedSharpAdbClient.AdbServer _adbServer { get; set; }

        /// <summary>
        /// The minimum version of <c>adb.exe</c> that is supported by this library.
        /// </summary>
        public static PackageVersion RequiredAdbVersion => AdvancedSharpAdbClient.AdbServer.RequiredAdbVersion.GetPackageVersion();

        /// <summary>
        /// Initializes a new instance of the <see cref="AdbServer"/> class.
        /// </summary>
        public AdbServer()
        {
            _adbServer = new(new AdvancedSharpAdbClient.AdbClient(), Factories.AdbCommandLineClientFactory);
        }

        /// <summary>
        /// Gets or sets the default instance of the <see cref="IAdbServer"/> interface.
        /// </summary>
        public static AdbServer Instance { get; set; } = new AdbServer();

        /// <summary>
        /// Starts the adb server if it was not previously running.
        /// </summary>
        /// <param name="adbPath">
        /// The path to the <c>adb.exe</c> executable that can be used to start the adb server.
        /// If this path is not provided, this method will throw an exception if the server
        /// is not running or is not up to date.
        /// </param>
        /// <param name="restartServerIfNewer">
        /// <see langword="true"/> to restart the adb server if the version of the <c>adb.exe</c>
        /// executable at <paramref name="adbPath"/> is newer than the version that is currently
        /// running; <see langword="false"/> to keep a previous version of the server running.
        /// </param>
        /// <returns>
        /// <list type="ordered">
        /// <item>
        ///     <see cref="StartServerResult.AlreadyRunning"/> if the adb server was already
        ///     running and the version of the adb server was at least <see cref="AdbServer.RequiredAdbVersion"/>.
        /// </item>
        /// <item>
        ///     <see cref="StartServerResult.RestartedOutdatedDaemon"/> if the adb server
        ///     was already running, but the version was less than <see cref="AdbServer.RequiredAdbVersion"/>
        ///     or less than the version of the adb client at <paramref name="adbPath"/> and the
        ///     <paramref name="restartServerIfNewer"/> flag was set.
        /// </item>
        /// <item>
        /// </item>
        ///     <see cref="StartServerResult.Started"/> if the adb server was not running,
        ///     and the server was started.
        /// </list>
        /// </returns>
        /// <exception cref="AdbException">
        /// The server was not running, or an outdated version of the server was running,
        /// and the <paramref name="adbPath"/> parameter was not specified.
        /// </exception>
        public StartServerResult StartServer(string adbPath, bool restartServerIfNewer) => (StartServerResult)_adbServer.StartServer(adbPath, restartServerIfNewer);

        /// <summary>
        /// Restarts the adb server if it suddenly became unavailable. Call this class if, for example,
        /// you receive an <see cref="AdbException"/> with the <see cref="AdbException.ConnectionReset"/> flag
        /// set to <see langword="true"/> - a clear indicating the ADB server died.
        /// </summary>
        /// <remarks>
        /// You can only call this method if you have previously started the adb server via
        /// <see cref="StartServer(string, bool)"/> and passed the full path to the adb server.
        /// </remarks>
        public void RestartServer() => _adbServer.RestartServer();

        /// <summary>
        /// Gets the status of the adb server.
        /// </summary>
        /// <returns>A <see cref="AdbServerStatus"/> object that describes the status of the adb server.</returns>
        public AdbServerStatus GetStatus() => AdbServerStatus.GetAdbServerStatus(_adbServer.GetStatus());
    }
}
