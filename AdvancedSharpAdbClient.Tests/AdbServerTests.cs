using NSubstitute;
using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Xunit;

namespace AdvancedSharpAdbClient.Tests
{
    /// <summary>
    /// Tests the <see cref="AdbServer"/> class.
    /// </summary>
    public partial class AdbServerTests
    {
        private readonly Func<string, IAdbCommandLineClient> adbCommandLineClientFactory;
        private readonly DummyAdbSocket socket;
        private readonly DummyAdbCommandLineClient commandLineClient;
        private Func<EndPoint, IAdbSocket> adbSocketFactory;
        private AdbServer adbServer;

        public AdbServerTests()
        {
            socket = new DummyAdbSocket();
            adbSocketFactory = endPoint => socket;

            commandLineClient = new DummyAdbCommandLineClient();
            adbCommandLineClientFactory = version => commandLineClient;

            adbServer = new AdbServer(adbSocketFactory, adbCommandLineClientFactory);
        }

        /// <summary>
        /// Tests the <see cref="AdbServer.GetStatus"/> method.
        /// </summary>
        [Fact]
        public void GetStatusNotRunningTest()
        {
            IAdbSocket adbSocketMock = Substitute.For<IAdbSocket>();
            adbSocketMock.When(x => x.SendAdbRequest("host:version")).Do(x => throw new SocketException(AdbServer.ConnectionRefused));

            AdbServer adbServer = new(endPoint => adbSocketMock, adbCommandLineClientFactory);

            AdbServerStatus status = adbServer.GetStatus();
            Assert.False(status.IsRunning);
            Assert.Null(status.Version);
        }

        /// <summary>
        /// Tests the <see cref="AdbServer.GetStatus"/> method.
        /// </summary>
        [Fact]
        public void GetStatusRunningTest()
        {
            socket.Responses.Enqueue(AdbResponse.OK);
            socket.ResponseMessages.Enqueue("0020");

            AdbServerStatus status = adbServer.GetStatus();

            Assert.Empty(socket.Responses);
            Assert.Empty(socket.ResponseMessages);
            Assert.Single(socket.Requests);
            Assert.Equal("host:version", socket.Requests[0]);

            Assert.True(status.IsRunning);
            Assert.Equal(new Version(1, 0, 32), status.Version);
        }

        /// <summary>
        /// Tests the <see cref="AdbServer.GetStatus"/> method.
        /// </summary>
        [Fact]
        public void GetStatusOtherSocketExceptionTest()
        {
            adbSocketFactory = endPoint => throw new SocketException();

            adbServer = new AdbServer(adbSocketFactory, adbCommandLineClientFactory);

            _ = Assert.Throws<SocketException>(() => adbServer.GetStatus());
        }

        /// <summary>
        /// Tests the <see cref="AdbServer.GetStatus"/> method.
        /// </summary>
        [Fact]
        public void GetStatusOtherExceptionTest()
        {
            adbSocketFactory = endPoint => throw new Exception();

            adbServer = new AdbServer(adbSocketFactory, adbCommandLineClientFactory);

            _ = Assert.Throws<Exception>(() => adbServer.GetStatus());
        }

        /// <summary>
        /// Tests the <see cref="AdbServer.StartServer(string, bool)"/> method.
        /// </summary>
        [Fact]
        public void StartServerAlreadyRunningTest()
        {
            commandLineClient.Version = AdbCommandLineStatus.GetVersionFromOutput(["Android Debug Bridge version 1.0.20"]);
            socket.Responses.Enqueue(AdbResponse.OK);
            socket.ResponseMessages.Enqueue("0020");

            StartServerResult result = adbServer.StartServer(null, false);

            Assert.Equal(StartServerResult.AlreadyRunning, result);

            Assert.Single(socket.Requests);
            Assert.Equal("host:version", socket.Requests[0]);
        }

        /// <summary>
        /// Tests the <see cref="AdbServer.StartServer(string, bool)"/> method.
        /// </summary>
        [Fact]
        public void StartServerOutdatedRunningNoExecutableTest()
        {
            socket.Responses.Enqueue(AdbResponse.OK);
            socket.ResponseMessages.Enqueue("0010");

            _ = Assert.Throws<AdbException>(() => adbServer.StartServer(null, false));
        }

        /// <summary>
        /// Tests the <see cref="AdbServer.StartServer(string, bool)"/> method.
        /// </summary>
        [Fact]
        public void StartServerNotRunningNoExecutableTest()
        {
            adbSocketFactory = endPoint => throw new SocketException(AdbServer.ConnectionRefused);

            adbServer = new AdbServer(adbSocketFactory, adbCommandLineClientFactory);

            _ = Assert.Throws<AdbException>(() => adbServer.StartServer(null, false));
        }

        /// <summary>
        /// Tests the <see cref="AdbServer.StartServer(string, bool)"/> method.
        /// </summary>
        [Fact]
        public void StartServerOutdatedRunningTest()
        {
            socket.Responses.Enqueue(AdbResponse.OK);
            socket.ResponseMessages.Enqueue("0010");

            commandLineClient.Version = AdbCommandLineStatus.GetVersionFromOutput(["Android Debug Bridge version 1.0.32"]);

            Assert.False(commandLineClient.ServerStarted);
            _ = adbServer.StartServer(ServerName, false);

            Assert.True(commandLineClient.ServerStarted);

            Assert.Equal(2, socket.Requests.Count);
            Assert.Equal("host:version", socket.Requests[0]);
            Assert.Equal("host:kill", socket.Requests[1]);
        }

        /// <summary>
        /// Tests the <see cref="AdbServer.StartServer(string, bool)"/> method.
        /// </summary>
        [Fact]
        public void StartServerNotRunningTest()
        {
            adbSocketFactory = endPoint => throw new SocketException(AdbServer.ConnectionRefused);

            adbServer = new AdbServer(adbSocketFactory, adbCommandLineClientFactory);

            commandLineClient.Version = AdbCommandLineStatus.GetVersionFromOutput(["Android Debug Bridge version 1.0.32"]);

            Assert.False(commandLineClient.ServerStarted);

            StartServerResult result = adbServer.StartServer(ServerName, false);

            Assert.True(commandLineClient.ServerStarted);
        }

        /// <summary>
        /// Tests the <see cref="AdbServer.StartServer(string, bool)"/> method.
        /// </summary>
        [Fact]
        public void StartServerIntermediateRestartRequestedRunningTest()
        {
            socket.Responses.Enqueue(AdbResponse.OK);
            socket.ResponseMessages.Enqueue("001f");

            commandLineClient.Version = AdbCommandLineStatus.GetVersionFromOutput(["Android Debug Bridge version 1.0.32"]);

            Assert.False(commandLineClient.ServerStarted);
            _ = adbServer.StartServer(ServerName, true);

            Assert.True(commandLineClient.ServerStarted);

            Assert.Equal(2, socket.Requests.Count);
            Assert.Equal("host:version", socket.Requests[0]);
            Assert.Equal("host:kill", socket.Requests[1]);
        }

        /// <summary>
        /// Tests the <see cref="AdbServer.StartServer(string, bool)"/> method.
        /// </summary>
        [Fact]
        public void StartServerIntermediateRestartNotRequestedRunningTest()
        {
            socket.Responses.Enqueue(AdbResponse.OK);
            socket.ResponseMessages.Enqueue("001f");

            commandLineClient.Version = AdbCommandLineStatus.GetVersionFromOutput(["Android Debug Bridge version 1.0.32"]);

            Assert.False(commandLineClient.ServerStarted);
            _ = adbServer.StartServer(ServerName, false);

            Assert.False(commandLineClient.ServerStarted);

            Assert.Single(socket.Requests);
            Assert.Equal("host:version", socket.Requests[0]);
        }

        /// <summary>
        /// Tests the <see cref="AdbServer.RestartServer(string)"/> method.
        /// </summary>
        [Fact]
        public void RestartServerTest()
        {
            socket.Responses.Enqueue(AdbResponse.OK);
            socket.ResponseMessages.Enqueue("001f");

            commandLineClient.Version = AdbCommandLineStatus.GetVersionFromOutput(["Android Debug Bridge version 1.0.32"]);

            Assert.False(commandLineClient.ServerStarted);
            _ = adbServer.RestartServer(ServerName);

            Assert.True(commandLineClient.ServerStarted);

            Assert.Equal(2, socket.Requests.Count);
            Assert.Equal("host:version", socket.Requests[0]);
            Assert.Equal("host:kill", socket.Requests[1]);
        }

        /// <summary>
        /// Tests the <see cref="AdbServer.StopServer"/> method.
        /// </summary>
        [Fact]
        public void StopServerTest()
        {
            adbServer.StopServer();

            Assert.Single(socket.Requests);
            Assert.Equal("host:kill", socket.Requests[0]);
        }

        /// <summary>
        /// Tests the <see cref="AdbServer.Clone()"/> method.
        /// </summary>
        [Fact]
        public void CloneTest()
        {
            DnsEndPoint endPoint = new("localhost", 5555);
            Assert.True(adbServer is ICloneable<IAdbServer>);
            AdbServer server = adbServer.Clone();
            Assert.Equal(adbServer.EndPoint, server.EndPoint);
            server = adbServer.Clone(endPoint);
            Assert.Equal(endPoint, server.EndPoint);
        }

        /// <summary>
        /// Tests the <see cref="AdbServer(EndPoint, Func{EndPoint, IAdbSocket}, Func{string, IAdbCommandLineClient})"/> method.
        /// </summary>
        [Fact]
        public void ConstructorAdbClientNullTest() =>
            _ = Assert.Throws<ArgumentNullException>(() => new AdbServer((EndPoint)null, adbSocketFactory, adbCommandLineClientFactory));

        private static string ServerName => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "adb.exe" : "adb";
    }
}
