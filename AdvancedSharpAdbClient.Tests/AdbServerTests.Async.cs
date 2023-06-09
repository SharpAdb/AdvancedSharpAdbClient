using AdvancedSharpAdbClient.Exceptions;
using Moq;
using System;
using System.Net.Sockets;
using System.Threading;
using Xunit;

namespace AdvancedSharpAdbClient.Tests
{
    public partial class AdbServerTests
    {
        [Fact]
        public async void GetStatusAsyncNotRunningTest()
        {
            Mock<IAdbClient> adbClientMock = new();
            adbClientMock.Setup(c => c.GetAdbVersionAsync(It.IsAny<CancellationToken>()))
                .Throws(new AggregateException(new SocketException(AdbServer.ConnectionRefused)));

            AdbServer adbServer = new(adbClientMock.Object, adbCommandLineClientFactory);

            AdbServerStatus status = await adbServer.GetStatusAsync();
            Assert.False(status.IsRunning);
            Assert.Null(status.Version);
        }

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

        [Fact]
        public async void GetStatusAsyncOtherSocketExceptionTest()
        {
            adbSocketFactory = (endPoint) => throw new SocketException();

            adbClient = new AdbClient(AdbClient.DefaultEndPoint, adbSocketFactory);
            adbServer = new AdbServer(adbClient, adbCommandLineClientFactory);

            _ = await Assert.ThrowsAsync<SocketException>(async () => await adbServer.GetStatusAsync());
        }

        [Fact]
        public async void GetStatusAsyncOtherExceptionTest()
        {
            adbSocketFactory = (endPoint) => throw new Exception();

            adbClient = new AdbClient(AdbClient.DefaultEndPoint, adbSocketFactory);
            adbServer = new AdbServer(adbClient, adbCommandLineClientFactory);

            _ = await Assert.ThrowsAsync<Exception>(async () => await adbServer.GetStatusAsync());
        }

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

        [Fact]
        public async void StartServerAsyncOutdatedRunningNoExecutableTest()
        {
            socket.Responses.Enqueue(AdbResponse.OK);
            socket.ResponseMessages.Enqueue("0010");

            _ = await Assert.ThrowsAsync<AdbException>(async () => await adbServer.StartServerAsync(null, false));
        }

        [Fact]
        public async void StartServerAsyncNotRunningNoExecutableTest()
        {
            adbSocketFactory = (endPoint) => throw new SocketException(AdbServer.ConnectionRefused);

            adbClient = new AdbClient(AdbClient.DefaultEndPoint, adbSocketFactory);
            adbServer = new AdbServer(adbClient, adbCommandLineClientFactory);

            _ = await Assert.ThrowsAsync<AdbException>(async () => await adbServer.StartServerAsync(null, false));
        }

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

        [Fact]
        public async void StartServerAsyncNotRunningTest()
        {
            adbSocketFactory = (endPoint) => throw new SocketException(AdbServer.ConnectionRefused);

            adbClient = new AdbClient(AdbClient.DefaultEndPoint, adbSocketFactory);
            adbServer = new AdbServer(adbClient, adbCommandLineClientFactory);

            commandLineClient.Version = new Version(1, 0, 32);

            Assert.False(commandLineClient.ServerStarted);

            StartServerResult result = await adbServer.StartServerAsync(ServerName, false);

            Assert.True(commandLineClient.ServerStarted);
        }

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

        [Fact]
        public async void RestartServerAsyncTest()
        {
            socket.Responses.Enqueue(AdbResponse.OK);
            socket.ResponseMessages.Enqueue("001f");

            commandLineClient.Version = new Version(1, 0, 32);

            Assert.False(commandLineClient.ServerStarted);
            _ = await adbServer.RestartServerAsync(ServerName);

            Assert.False(commandLineClient.ServerStarted);

            Assert.Single(socket.Requests);
            Assert.Equal("host:version", socket.Requests[0]);
        }
    }
}
