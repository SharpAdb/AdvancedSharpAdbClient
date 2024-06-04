using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AdvancedSharpAdbClient.Tests
{
    public partial class DeviceMonitorTests
    {
        [Fact]
        public async Task DeviceDisconnectedAsyncTest()
        {
            Socket.WaitForNewData = true;

            await using DeviceMonitor monitor = new(Socket);
            DeviceMonitorSink sink = new(monitor);

            Assert.Empty(monitor.Devices);

            // Start the monitor, detect the initial device.
            await RunTestAsync(
                OkResponse,
                ["169.254.109.177:5555\tdevice\n"],
                ["host:track-devices"],
                () => monitor.StartAsync());

            Assert.Single(monitor.Devices);
            Assert.Single(sink.ConnectedEvents);
            Assert.Empty(sink.ChangedEvents);
            Assert.Single(sink.NotifiedEvents);
            Assert.Single(sink.ListChangedEvents);
            Assert.Empty(sink.DisconnectedEvents);

            Socket.ResponseMessages.Clear();
            Socket.Responses.Clear();
            Socket.Requests.Clear();

            sink.ResetSignals();

            // Device disconnects
            ManualResetEvent eventWaiter = sink.CreateEventSignal();

            _ = await RunTestAsync(
                NoResponses,
                [string.Empty],
                NoRequests,
                () => Task.Run(() => eventWaiter.WaitOne(1000)));

            Assert.Empty(monitor.Devices);
            Assert.Empty(sink.ConnectedEvents);
            Assert.Empty(sink.ChangedEvents);
            Assert.Single(sink.NotifiedEvents);
            Assert.Single(sink.ListChangedEvents);
            Assert.Single(sink.DisconnectedEvents);
            Assert.Equal("169.254.109.177:5555", sink.DisconnectedEvents[0].Device.Serial);
        }

        [Fact]
        public async Task DeviceConnectedAsyncTest()
        {
            Socket.WaitForNewData = true;

            await using DeviceMonitor monitor = new(Socket);
            DeviceMonitorSink sink = new(monitor);

            Assert.Empty(monitor.Devices);

            // Start the monitor, detect the initial device.
            await RunTestAsync(
                OkResponse,
                [string.Empty],
                ["host:track-devices"],
                () => monitor.StartAsync());

            Assert.Empty(monitor.Devices);
            Assert.Empty(sink.ConnectedEvents);
            Assert.Empty(sink.ChangedEvents);
            Assert.Single(sink.NotifiedEvents);
            Assert.Empty(sink.ListChangedEvents);
            Assert.Empty(sink.DisconnectedEvents);

            Socket.ResponseMessages.Clear();
            Socket.Responses.Clear();
            Socket.Requests.Clear();

            sink.ResetSignals();

            // Device disconnects
            ManualResetEvent eventWaiter = sink.CreateEventSignal();

            _ = await RunTestAsync(
                NoResponses,
                ["169.254.109.177:5555\tdevice\n"],
                NoRequests,
                () => Task.Run(() => eventWaiter.WaitOne(1000)));

            Assert.Single(monitor.Devices);
            Assert.Single(sink.ConnectedEvents);
            Assert.Empty(sink.ChangedEvents);
            Assert.Single(sink.NotifiedEvents);
            Assert.Single(sink.ListChangedEvents);
            Assert.Empty(sink.DisconnectedEvents);
            Assert.Equal("169.254.109.177:5555", sink.ConnectedEvents[0].Device.Serial);
        }

        /// <summary>
        /// Tests the <see cref="DeviceMonitor.StartAsync(CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task StartInitialDeviceListAsyncTest()
        {
            Socket.WaitForNewData = true;

            await using DeviceMonitor monitor = new(Socket);
            DeviceMonitorSink sink = new(monitor);

            Assert.Empty(monitor.Devices);

            await RunTestAsync(
                OkResponse,
                ["169.254.109.177:5555\tdevice\n"],
                ["host:track-devices"],
                () => monitor.StartAsync());

            Assert.Single(monitor.Devices);
            Assert.Equal("169.254.109.177:5555", monitor.Devices[0].Serial);
            Assert.Single(sink.ConnectedEvents);
            Assert.Equal("169.254.109.177:5555", sink.ConnectedEvents[0].Device.Serial);
            Assert.Empty(sink.ChangedEvents);
            Assert.Single(sink.NotifiedEvents);
            Assert.Single(sink.ListChangedEvents);
            Assert.Empty(sink.DisconnectedEvents);
        }

        [Fact]
        public async Task TriggeredWhenStatusChangedAsyncTest()
        {
            Socket.WaitForNewData = true;

            await using DeviceMonitor monitor = new(Socket);
            DeviceMonitorSink sink = new(monitor);

            Assert.Empty(monitor.Devices);

            // Start the monitor, detect the initial device.
            await RunTestAsync(
                OkResponse,
                ["169.254.109.177:5555\toffline\n"],
                ["host:track-devices"],
                () => monitor.StartAsync());

            Assert.Single(monitor.Devices);
            Assert.Equal(DeviceState.Offline, monitor.Devices[0].State);
            Assert.Single(sink.ConnectedEvents);
            Assert.Empty(sink.ChangedEvents);
            Assert.Single(sink.NotifiedEvents);
            Assert.Single(sink.ListChangedEvents);
            Assert.Empty(sink.DisconnectedEvents);

            Socket.ResponseMessages.Clear();
            Socket.Responses.Clear();
            Socket.Requests.Clear();

            sink.ResetSignals();

            // Device disconnects
            ManualResetEvent eventWaiter = sink.CreateEventSignal();

            _ = await RunTestAsync(
                NoResponses,
                ["169.254.109.177:5555\tdevice\n"],
                NoRequests,
                () => Task.Run(() => eventWaiter.WaitOne(1000)));

            Assert.Single(monitor.Devices);
            Assert.Equal(DeviceState.Online, monitor.Devices[0].State);
            Assert.Empty(sink.ConnectedEvents);
            Assert.Single(sink.ChangedEvents);
            Assert.Single(sink.NotifiedEvents);
            Assert.Single(sink.ListChangedEvents);
            Assert.Empty(sink.DisconnectedEvents);
            Assert.Equal("169.254.109.177:5555", sink.ChangedEvents[0].Device.Serial);
        }

        [Fact]
        public async Task NoTriggerIfStatusIsSameAsyncTest()
        {
            Socket.WaitForNewData = true;

            await using DeviceMonitor monitor = new(Socket);
            DeviceMonitorSink sink = new(monitor);

            Assert.Empty(monitor.Devices);

            // Start the monitor, detect the initial device.
            await RunTestAsync(
                OkResponse,
                ["169.254.109.177:5555\toffline\n"],
                ["host:track-devices"],
                () => monitor.StartAsync());

            Assert.Single(monitor.Devices);
            Assert.Equal(DeviceState.Offline, monitor.Devices.ElementAt(0).State);
            Assert.Single(sink.ConnectedEvents);
            Assert.Empty(sink.ChangedEvents);
            Assert.Single(sink.NotifiedEvents);
            Assert.Single(sink.ListChangedEvents);
            Assert.Empty(sink.DisconnectedEvents);

            Socket.ResponseMessages.Clear();
            Socket.Responses.Clear();
            Socket.Requests.Clear();

            sink.ResetSignals();

            // Something happens but device does not change
            ManualResetEvent eventWaiter = sink.CreateEventSignal();

            _ = await RunTestAsync(
                NoResponses,
                ["169.254.109.177:5555\toffline\n"],
                NoRequests,
                () => Task.Run(() => eventWaiter.WaitOne(1000)));

            Assert.Single(monitor.Devices);
            Assert.Equal(DeviceState.Offline, monitor.Devices.ElementAt(0).State);
            Assert.Empty(sink.ConnectedEvents);
            Assert.Empty(sink.ChangedEvents);
            Assert.Single(sink.NotifiedEvents);
            Assert.Empty(sink.ListChangedEvents);
            Assert.Empty(sink.DisconnectedEvents);
        }

        /// <summary>
        /// Tests the <see cref="DeviceMonitor"/> in a case where the adb server dies in the middle of the monitor
        /// loop. The <see cref="DeviceMonitor"/> should detect this condition and restart the adb server.
        /// </summary>
        [Fact]
        public async Task AdbKilledAsyncTest()
        {
            DummyAdbServer dummyAdbServer = new();
            AdbServer.Instance = dummyAdbServer;

            Socket.WaitForNewData = true;

            await using DeviceMonitor monitor = new(Socket);
            await RunTestAsync(
                OkResponses(2),
                [DummyAdbSocket.ServerDisconnected, string.Empty],
                ["host:track-devices", "host:track-devices"],
                () => monitor.StartAsync());

            Assert.True(Socket.DidReconnect);
            Assert.True(dummyAdbServer.WasRestarted);
        }
    }
}
