using NSubstitute;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using Xunit;

namespace AdvancedSharpAdbClient.Tests
{
    public partial class AdbClientExtensionsTests
    {
        [Fact]
        public async void ExecuteServerCommandAsyncTest()
        {
            const string target = nameof(target);
            const string command = nameof(command);
            IAdbSocket socket = Substitute.For<IAdbSocket>();
            IShellOutputReceiver receiver = Substitute.For<IShellOutputReceiver>();

            IAdbClient client = Substitute.For<IAdbClient>();
            client.When(x => x.ExecuteServerCommandAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IShellOutputReceiver>(), Arg.Any<Encoding>(), Arg.Any<CancellationToken>()))
                .Do(x =>
                {
                    Assert.Equal(target, x.ArgAt<string>(0));
                    Assert.Equal(command, x.ArgAt<string>(1));
                    Assert.Equal(receiver, x.ArgAt<IShellOutputReceiver>(2));
                    Assert.Equal(AdbClient.Encoding, x.ArgAt<Encoding>(3));
                    Assert.Equal(default, x.ArgAt<CancellationToken>(4));
                });
            client.When(x => x.ExecuteServerCommandAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IAdbSocket>(), Arg.Any<IShellOutputReceiver>(), Arg.Any<Encoding>(), Arg.Any<CancellationToken>()))
                .Do(x =>
                {
                    Assert.Equal(target, x.ArgAt<string>(0));
                    Assert.Equal(command, x.ArgAt<string>(1));
                    Assert.Equal(socket, x.ArgAt<IAdbSocket>(2));
                    Assert.Equal(receiver, x.ArgAt<IShellOutputReceiver>(3));
                    Assert.Equal(AdbClient.Encoding, x.ArgAt<Encoding>(4));
                    Assert.Equal(default, x.ArgAt<CancellationToken>(5));
                });

            await client.ExecuteServerCommandAsync(target, command, receiver);
            await client.ExecuteServerCommandAsync(target, command, socket, receiver);
        }

        [Fact]
        public async void ExecuteRemoteCommandAsyncTest()
        {
            const string command = nameof(command);
            DeviceData device = new();
            IShellOutputReceiver receiver = Substitute.For<IShellOutputReceiver>();

            IAdbClient client = Substitute.For<IAdbClient>();
            client.When(x => x.ExecuteRemoteCommandAsync(Arg.Any<string>(), Arg.Any<DeviceData>(), Arg.Any<IShellOutputReceiver>(), Arg.Any<Encoding>(), Arg.Any<CancellationToken>()))
                .Do(x =>
                {
                    Assert.Equal(command, x.ArgAt<string>(0));
                    Assert.Equal(device, x.ArgAt<DeviceData>(1));
                    Assert.Equal(receiver, x.ArgAt<IShellOutputReceiver>(2));
                    Assert.Equal(AdbClient.Encoding, x.ArgAt<Encoding>(3));
                    Assert.Equal(default, x.ArgAt<CancellationToken>(4));
                });

            await client.ExecuteRemoteCommandAsync(command, device, receiver);
        }

        [Fact]
        public async void RunLogServiceAsyncTest()
        {
            DeviceData device = new();
            IProgress<LogEntry> progress = Substitute.For<IProgress<LogEntry>>();
            Action<LogEntry> messageSink = progress.Report;
            LogId[] logNames = Enumerable.Range((int)LogId.Min, (int)(LogId.Max - LogId.Min + 1)).Select(x => (LogId)x).ToArray();

            IAdbClient client = Substitute.For<IAdbClient>();
            client.When(x => x.RunLogServiceAsync(Arg.Any<DeviceData>(), Arg.Any<Action<LogEntry>>(), Arg.Any<CancellationToken>(), Arg.Any<LogId[]>()))
                .Do(x =>
                {
                    Assert.Equal(device, x.ArgAt<DeviceData>(0));
                    Assert.Equal(messageSink, x.ArgAt<Action<LogEntry>>(1));
                    Assert.Equal(default, x.ArgAt<CancellationToken>(2));
                    Assert.Equal(logNames, x.ArgAt<LogId[]>(3));
                });

            await client.RunLogServiceAsync(device, messageSink, logNames);
            await client.RunLogServiceAsync(device, progress, default, logNames);
            await client.RunLogServiceAsync(device, progress, logNames);
        }
    }
}
