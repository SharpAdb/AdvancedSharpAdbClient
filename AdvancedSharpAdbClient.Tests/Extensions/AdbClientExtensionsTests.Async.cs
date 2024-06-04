using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AdvancedSharpAdbClient.Tests
{
    public partial class AdbClientExtensionsTests
    {
        [Fact]
        public async Task ExecuteServerCommandAsyncTest()
        {
            const string target = nameof(target);
            const string command = nameof(command);
            IAdbSocket socket = Substitute.For<IAdbSocket>();
            static bool predicate(string x) => true;
            IShellOutputReceiver receiver = new FunctionOutputReceiver(predicate);
            Encoding encoding = AdbClient.Encoding;
            List<string> result = ["Hello", "World", "!"];

            IAdbClient client = Substitute.For<IAdbClient>();
            _ = client.ExecuteServerCommandAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IShellOutputReceiver>(), Arg.Any<Encoding>(), Arg.Any<CancellationToken>())
                .Returns(x =>
                {
                    Assert.Equal(target, x.ArgAt<string>(0));
                    Assert.Equal(command, x.ArgAt<string>(1));
                    Assert.Equal(receiver, x.ArgAt<IShellOutputReceiver>(2));
                    Assert.Equal(encoding, x.ArgAt<Encoding>(3));
                    Assert.Equal(default, x.ArgAt<CancellationToken>(4));
                    return Task.CompletedTask;
                });
            _ = client.ExecuteServerCommandAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IAdbSocket>(), Arg.Any<IShellOutputReceiver>(), Arg.Any<Encoding>(), Arg.Any<CancellationToken>())
                .Returns(x =>
                {
                    Assert.Equal(target, x.ArgAt<string>(0));
                    Assert.Equal(command, x.ArgAt<string>(1));
                    Assert.Equal(socket, x.ArgAt<IAdbSocket>(2));
                    Assert.Equal(receiver, x.ArgAt<IShellOutputReceiver>(3));
                    Assert.Equal(encoding, x.ArgAt<Encoding>(4));
                    Assert.Equal(default, x.ArgAt<CancellationToken>(5));
                    return Task.CompletedTask;
                });
            _ = client.ExecuteServerCommandAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Encoding>(), Arg.Any<CancellationToken>())
                .Returns(x =>
                {
                    Assert.Equal(target, x.ArgAt<string>(0));
                    Assert.Equal(command, x.ArgAt<string>(1));
                    Assert.Equal(encoding, x.ArgAt<Encoding>(2));
                    Assert.Equal(default, x.ArgAt<CancellationToken>(3));
                    return result.AsEnumerableAsync(x.ArgAt<CancellationToken>(3));
                });
            _ = client.ExecuteServerCommandAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IAdbSocket>(), Arg.Any<Encoding>(), Arg.Any<CancellationToken>())
                .Returns(x =>
                {
                    Assert.Equal(target, x.ArgAt<string>(0));
                    Assert.Equal(command, x.ArgAt<string>(1));
                    Assert.Equal(socket, x.ArgAt<IAdbSocket>(2));
                    Assert.Equal(encoding, x.ArgAt<Encoding>(3));
                    Assert.Equal(default, x.ArgAt<CancellationToken>(4));
                    return result.AsEnumerableAsync(x.ArgAt<CancellationToken>(4));
                });

            await client.ExecuteServerCommandAsync(target, command, receiver);
            await client.ExecuteServerCommandAsync(target, command, socket, receiver);
            await client.ExecuteServerCommandAsync(target, command, predicate);
            await client.ExecuteServerCommandAsync(target, command, socket, predicate);
            await client.ExecuteServerCommandAsync(target, command, predicate, encoding);
            await client.ExecuteServerCommandAsync(target, command, socket, predicate, encoding);
            Assert.Equal(result, await AdbClientExtensions.ExecuteServerCommandAsync(client, target, command).ToListAsync());
            Assert.Equal(result, await AdbClientExtensions.ExecuteServerCommandAsync(client, target, command, socket).ToListAsync());
        }

        [Fact]
        public async Task ExecuteRemoteCommandAsyncTest()
        {
            const string command = nameof(command);
            DeviceData device = new() { Serial = "169.254.109.177:5555" };
            static bool predicate(string x) => true;
            IShellOutputReceiver receiver = new FunctionOutputReceiver(predicate);
            Encoding encoding = AdbClient.Encoding;
            List<string> result = ["Hello", "World", "!"];

            IAdbClient client = Substitute.For<IAdbClient>();
            _ = client.ExecuteRemoteCommandAsync(Arg.Any<string>(), device, Arg.Any<IShellOutputReceiver>(), Arg.Any<Encoding>(), Arg.Any<CancellationToken>())
                .Returns(x =>
                {
                    Assert.Equal(command, x.ArgAt<string>(0));
                    Assert.Equal(device, x.ArgAt<DeviceData>(1));
                    Assert.Equal(receiver, x.ArgAt<IShellOutputReceiver>(2));
                    Assert.Equal(encoding, x.ArgAt<Encoding>(3));
                    Assert.Equal(default, x.ArgAt<CancellationToken>(4));
                    return Task.CompletedTask;
                });
            _ = client.ExecuteRemoteCommandAsync(Arg.Any<string>(), device, Arg.Any<Encoding>(), Arg.Any<CancellationToken>())
                .Returns(x =>
                {
                    Assert.Equal(command, x.ArgAt<string>(0));
                    Assert.Equal(device, x.ArgAt<DeviceData>(1));
                    Assert.Equal(encoding, x.ArgAt<Encoding>(2));
                    Assert.Equal(default, x.ArgAt<CancellationToken>(3));
                    return result.AsEnumerableAsync(x.ArgAt<CancellationToken>(3));
                });

            await client.ExecuteRemoteCommandAsync(command, device, receiver);
            await client.ExecuteRemoteCommandAsync(command, device, predicate);
            await client.ExecuteRemoteCommandAsync(command, device, predicate, encoding);
            Assert.Equal(result, await AdbClientExtensions.ExecuteRemoteCommandAsync(client, command, device).ToListAsync());
        }

        [Fact]
        public async Task RunLogServiceAsyncTest()
        {
            DeviceData device = new() { Serial = "169.254.109.177:5555" };
            IProgress<LogEntry> progress = Substitute.For<IProgress<LogEntry>>();
            Action<LogEntry> messageSink = progress.Report;
            LogId[] logNames = Enumerable.Range((int)LogId.Min, (int)(LogId.Max - LogId.Min + 1)).Select(x => (LogId)x).ToArray();

            IAdbClient client = Substitute.For<IAdbClient>();
            _ = client.RunLogServiceAsync(device, Arg.Any<Action<LogEntry>>(), Arg.Any<CancellationToken>(), Arg.Any<LogId[]>())
                .Returns(x =>
                {
                    Assert.Equal(device, x.ArgAt<DeviceData>(0));
                    Assert.Equal(messageSink, x.ArgAt<Action<LogEntry>>(1));
                    Assert.Equal(default, x.ArgAt<CancellationToken>(2));
                    Assert.Equal(logNames, x.ArgAt<LogId[]>(3));
                    return Task.CompletedTask;
                });

            await client.RunLogServiceAsync(device, messageSink, logNames);
            await client.RunLogServiceAsync(device, progress, default, logNames);
            await client.RunLogServiceAsync(device, progress, logNames);
        }
    }
}
