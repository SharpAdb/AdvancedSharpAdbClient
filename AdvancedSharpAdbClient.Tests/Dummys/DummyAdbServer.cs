using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace AdvancedSharpAdbClient.Tests
{
    /// <summary>
    /// A mock implementation of the <see cref="IAdbServer"/> class.
    /// </summary>
    internal class DummyAdbServer : IAdbServer
    {
        /// <inheritdoc/>
        /// <remarks>
        /// The value is set to a value different from the default adb end point, to detect the dummy
        /// server being used. 
        /// </remarks>
        public EndPoint EndPoint { get; set; } = new IPEndPoint(IPAddress.Loopback, 9999);

        /// <summary>
        /// Gets or sets the status as is to be reported by the <see cref="DummyAdbServer"/>.
        /// </summary>
        public AdbServerStatus Status { get; set; }

        /// <summary>
        /// Gets a value indicating whether the server was restarted.
        /// </summary>
        public bool WasRestarted { get; private set; }

        /// <inheritdoc/>
        public AdbServerStatus GetStatus() => Status;

        /// <inheritdoc/>
        public async Task<AdbServerStatus> GetStatusAsync(CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            return Status;
        }

        /// <inheritdoc/>
        public StartServerResult RestartServer() => RestartServer(null);

        /// <inheritdoc/>
        public StartServerResult RestartServer(string adbPath)
        {
            WasRestarted = true;
            return StartServer(adbPath, false);
        }

        /// <inheritdoc/>
        public Task<StartServerResult> RestartServerAsync(CancellationToken cancellationToken = default) => RestartServerAsync(null, cancellationToken);

        /// <inheritdoc/>
        public Task<StartServerResult> RestartServerAsync(string adbPath, CancellationToken cancellationToken = default)
        {
            WasRestarted = true;
            return StartServerAsync(adbPath, false, cancellationToken);
        }

        /// <inheritdoc/>
        public StartServerResult StartServer(string adbPath, bool restartServerIfNewer)
        {
            if (Status.IsRunning == true)
            {
                return StartServerResult.AlreadyRunning;
            }
            Status = new AdbServerStatus(true, new Version(1, 0, 20));
            return StartServerResult.Started;
        }

        /// <inheritdoc/>
        public async Task<StartServerResult> StartServerAsync(string adbPath, bool restartServerIfNewer, CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            return StartServer(adbPath, restartServerIfNewer);
        }

        /// <inheritdoc/>
        public void StopServer() => Status = Status with { IsRunning = false };

        /// <inheritdoc/>
        public async Task StopServerAsync(CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            StopServer();
        }
    }
}
