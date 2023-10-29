using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System;
using System.Net.Sockets;
using System.Threading;
using Xunit;

namespace AdvancedSharpAdbClient.Tests
{
    public partial class AdbServerTests
    {
        /// <summary>
        /// Tests the <see cref="AdbServer.GetStatusAsync(CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void GetStatusAsyncNotRunningTest()
        {
            IAdbSocket adbSocketMock = Substitute.For<IAdbSocket>();
            adbSocketMock.SendAdbRequestAsync("host:version", Arg.Any<CancellationToken>()).Throws(new AggregateException(new SocketException(AdbServer.ConnectionRefused)));

            AdbServer adbServer = new((endPoint) => adbSocketMock, adbCommandLineClientFactory);

            AdbServerStatus status = await adbServer.GetStatusAsync();
            Assert.False(status.IsRunning);
            Assert.Null(status.Version);
        }

        /// <summary>
        /// Tests the <see cref="AdbServer.GetStatusAsync(CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void GetStatusAsyncRunningTest()
        {
            socket.Responses.Enqueue(AdbResponse.OK);
            socket.ResponseMessages.Enqueue("0020");

            AdbServerStatus status = await adbServer.GetStatusAsync();

            Assert.Empty(socket.Responses);
            Assert.Empty(socket.ResponseMessages);
            Assert.Single(socket.Requests);
            Assert.Equal("host:version", socket.Requests[0]);

            Assert.True(status.IsRunning);
            Assert.Equal(new Version(1, 0, 32), status.Version);
        }

        /// <summary>
        /// Tests the <see cref="AdbServer.GetStatusAsync(CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void GetStatusAsyncOtherSocketExceptionTest()
        {
            adbSocketFactory = (endPoint) => throw new SocketException();

            adbServer = new AdbServer(adbSocketFactory, adbCommandLineClientFactory);

            _ = await Assert.ThrowsAsync<SocketException>(async () => await adbServer.GetStatusAsync());
        }

        /// <summary>
        /// Tests the <see cref="AdbServer.GetStatusAsync(CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void GetStatusAsyncOtherExceptionTest()
        {
            adbSocketFactory = (endPoint) => throw new Exception();

            adbServer = new AdbServer(adbSocketFactory, adbCommandLineClientFactory);

            _ = await Assert.ThrowsAsync<Exception>(async () => await adbServer.GetStatusAsync());
        }

        /// <summary>
        /// Tests the <see cref="AdbServer.StartServerAsync(string, bool, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void StartServerAsyncAlreadyRunningTest()
        {
            commandLineClient.Version = new Version(1, 0, 20);
            socket.Responses.Enqueue(AdbResponse.OK);
            socket.ResponseMessages.Enqueue("0020");

            StartServerResult result = await adbServer.StartServerAsync(null, false);

            Assert.Equal(StartServerResult.AlreadyRunning, result);

            Assert.Single(socket.Requests);
            Assert.Equal("host:version", socket.Requests[0]);
        }

        /// <summary>
        /// Tests the <see cref="AdbServer.StartServerAsync(string, bool, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void StartServerAsyncOutdatedRunningNoExecutableTest()
        {
            socket.Responses.Enqueue(AdbResponse.OK);
            socket.ResponseMessages.Enqueue("0010");

            _ = await Assert.ThrowsAsync<AdbException>(async () => await adbServer.StartServerAsync(null, false));
        }

        /// <summary>
        /// Tests the <see cref="AdbServer.StartServerAsync(string, bool, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void StartServerAsyncNotRunningNoExecutableTest()
        {
            adbSocketFactory = (endPoint) => throw new SocketException(AdbServer.ConnectionRefused);

            adbServer = new AdbServer(adbSocketFactory, adbCommandLineClientFactory);

            _ = await Assert.ThrowsAsync<AdbException>(async () => await adbServer.StartServerAsync(null, false));
        }

        /// <summary>
        /// Tests the <see cref="AdbServer.StartServerAsync(string, bool, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void StartServerAsyncOutdatedRunningTest()
        {
            socket.Responses.Enqueue(AdbResponse.OK);
            socket.ResponseMessages.Enqueue("0010");

            commandLineClient.Version = new Version(1, 0, 32);

            Assert.False(commandLineClient.ServerStarted);
            _ = await adbServer.StartServerAsync(ServerName, false);

            Assert.True(commandLineClient.ServerStarted);

            Assert.Equal(2, socket.Requests.Count);
            Assert.Equal("host:version", socket.Requests[0]);
            Assert.Equal("host:kill", socket.Requests[1]);
        }

        /// <summary>
        /// Tests the <see cref="AdbServer.StartServerAsync(string, bool, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void StartServerAsyncNotRunningTest()
        {
            adbSocketFactory = (endPoint) => throw new SocketException(AdbServer.ConnectionRefused);

            adbServer = new AdbServer(adbSocketFactory, adbCommandLineClientFactory);

            commandLineClient.Version = new Version(1, 0, 32);

            Assert.False(commandLineClient.ServerStarted);

            StartServerResult result = await adbServer.StartServerAsync(ServerName, false);

            Assert.True(commandLineClient.ServerStarted);
        }

        /// <summary>
        /// Tests the <see cref="AdbServer.StartServerAsync(string, bool, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void StartServerAsyncIntermediateRestartRequestedRunningTest()
        {
            socket.Responses.Enqueue(AdbResponse.OK);
            socket.ResponseMessages.Enqueue("001f");

            commandLineClient.Version = new Version(1, 0, 32);

            Assert.False(commandLineClient.ServerStarted);
            _ = await adbServer.StartServerAsync(ServerName, true);

            Assert.True(commandLineClient.ServerStarted);

            Assert.Equal(2, socket.Requests.Count);
            Assert.Equal("host:version", socket.Requests[0]);
            Assert.Equal("host:kill", socket.Requests[1]);
        }

        /// <summary>
        /// Tests the <see cref="AdbServer.StartServerAsync(string, bool, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void StartServerAsyncIntermediateRestartNotRequestedRunningTest()
        {
            socket.Responses.Enqueue(AdbResponse.OK);
            socket.ResponseMessages.Enqueue("001f");

            commandLineClient.Version = new Version(1, 0, 32);

            Assert.False(commandLineClient.ServerStarted);
            _ = await adbServer.StartServerAsync(ServerName, false);

            Assert.False(commandLineClient.ServerStarted);

            Assert.Single(socket.Requests);
            Assert.Equal("host:version", socket.Requests[0]);
        }

        /// <summary>
        /// Tests the <see cref="AdbServer.RestartServerAsync(string, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void RestartServerAsyncTest()
        {
            socket.Responses.Enqueue(AdbResponse.OK);
            socket.ResponseMessages.Enqueue("001f");

            commandLineClient.Version = new Version(1, 0, 32);

            Assert.False(commandLineClient.ServerStarted);
            _ = await adbServer.RestartServerAsync(ServerName);

            Assert.True(commandLineClient.ServerStarted);

            Assert.Equal(2, socket.Requests.Count);
            Assert.Equal("host:version", socket.Requests[0]);
            Assert.Equal("host:kill", socket.Requests[1]);
        }

        /// <summary>
        /// Tests the <see cref="AdbServer.StopServerAsync(CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void StopServerAsyncTest()
        {
            await adbServer.StopServerAsync();

            Assert.Single(socket.Requests);
            Assert.Equal("host:kill", socket.Requests[0]);
        }
    }
}
