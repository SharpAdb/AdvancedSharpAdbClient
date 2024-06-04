using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AdvancedSharpAdbClient.Tests
{
    public partial class AdbServerTests
    {
        /// <summary>
        /// Tests the <see cref="AdbServer.GetStatusAsync(CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task GetStatusAsyncNotRunningTest()
        {
            IAdbSocket adbSocketMock = Substitute.For<IAdbSocket>();
            adbSocketMock.SendAdbRequestAsync("host:version", Arg.Any<CancellationToken>()).Throws(new AggregateException(new SocketException(AdbServer.ConnectionRefused)));

            AdbServer adbServer = new(endPoint => adbSocketMock, adbCommandLineClientFactory);

            AdbServerStatus status = await adbServer.GetStatusAsync();
            Assert.False(status.IsRunning);
            Assert.Null(status.Version);
        }

        /// <summary>
        /// Tests the <see cref="AdbServer.GetStatusAsync(CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task GetStatusAsyncRunningTest()
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
        public async Task GetStatusAsyncOtherSocketExceptionTest()
        {
            adbSocketFactory = endPoint => throw new SocketException();

            adbServer = new AdbServer(adbSocketFactory, adbCommandLineClientFactory);

            _ = await Assert.ThrowsAsync<SocketException>(async () => await adbServer.GetStatusAsync());
        }

        /// <summary>
        /// Tests the <see cref="AdbServer.GetStatusAsync(CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task GetStatusAsyncOtherExceptionTest()
        {
            adbSocketFactory = endPoint => throw new Exception();

            adbServer = new AdbServer(adbSocketFactory, adbCommandLineClientFactory);

            _ = await Assert.ThrowsAsync<Exception>(async () => await adbServer.GetStatusAsync());
        }

        /// <summary>
        /// Tests the <see cref="AdbServer.StartServerAsync(string, bool, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task StartServerAsyncAlreadyRunningTest()
        {
            commandLineClient.Version = AdbCommandLineStatus.GetVersionFromOutput(["Android Debug Bridge version 1.0.20"]);
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
        public async Task StartServerAsyncOutdatedRunningNoExecutableTest()
        {
            socket.Responses.Enqueue(AdbResponse.OK);
            socket.ResponseMessages.Enqueue("0010");

            AggregateException exception = await Assert.ThrowsAsync<AggregateException>(async () => await adbServer.StartServerAsync(null, false));
            Assert.IsType<AdbException>(exception.InnerException);
        }

        /// <summary>
        /// Tests the <see cref="AdbServer.StartServerAsync(string, bool, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task StartServerAsyncNotRunningNoExecutableTest()
        {
            adbSocketFactory = endPoint => throw new SocketException(AdbServer.ConnectionRefused);

            adbServer = new AdbServer(adbSocketFactory, adbCommandLineClientFactory);

            AggregateException exception = await Assert.ThrowsAsync<AggregateException>(async () => await adbServer.StartServerAsync(null, false));
            Assert.IsType<AdbException>(exception.InnerException);
        }

        /// <summary>
        /// Tests the <see cref="AdbServer.StartServerAsync(string, bool, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task StartServerAsyncOutdatedRunningTest()
        {
            socket.Responses.Enqueue(AdbResponse.OK);
            socket.ResponseMessages.Enqueue("0010");

            commandLineClient.Version = AdbCommandLineStatus.GetVersionFromOutput(["Android Debug Bridge version 1.0.32"]);

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
        public async Task StartServerAsyncNotRunningTest()
        {
            adbSocketFactory = endPoint => throw new SocketException(AdbServer.ConnectionRefused);

            adbServer = new AdbServer(adbSocketFactory, adbCommandLineClientFactory);

            commandLineClient.Version = AdbCommandLineStatus.GetVersionFromOutput(["Android Debug Bridge version 1.0.32"]);

            Assert.False(commandLineClient.ServerStarted);

            StartServerResult result = await adbServer.StartServerAsync(ServerName, false);

            Assert.True(commandLineClient.ServerStarted);
        }

        /// <summary>
        /// Tests the <see cref="AdbServer.StartServerAsync(string, bool, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task StartServerAsyncIntermediateRestartRequestedRunningTest()
        {
            socket.Responses.Enqueue(AdbResponse.OK);
            socket.ResponseMessages.Enqueue("001f");

            commandLineClient.Version = AdbCommandLineStatus.GetVersionFromOutput(["Android Debug Bridge version 1.0.32"]);

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
        public async Task StartServerAsyncIntermediateRestartNotRequestedRunningTest()
        {
            socket.Responses.Enqueue(AdbResponse.OK);
            socket.ResponseMessages.Enqueue("001f");

            commandLineClient.Version = AdbCommandLineStatus.GetVersionFromOutput(["Android Debug Bridge version 1.0.32"]);

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
        public async Task RestartServerAsyncTest()
        {
            socket.Responses.Enqueue(AdbResponse.OK);
            socket.ResponseMessages.Enqueue("001f");

            commandLineClient.Version = AdbCommandLineStatus.GetVersionFromOutput(["Android Debug Bridge version 1.0.32"]);

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
        public async Task StopServerAsyncTest()
        {
            await adbServer.StopServerAsync();

            Assert.Single(socket.Requests);
            Assert.Equal("host:kill", socket.Requests[0]);
        }
    }
}
