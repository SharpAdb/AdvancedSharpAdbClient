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
                    Assert.Equal(TestContext.Current.CancellationToken, x.ArgAt<CancellationToken>(4));
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
                    Assert.Equal(TestContext.Current.CancellationToken, x.ArgAt<CancellationToken>(5));
                    return Task.CompletedTask;
                });
            _ = client.ExecuteServerEnumerableAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Encoding>(), Arg.Any<CancellationToken>())
                .Returns(x =>
                {
                    Assert.Equal(target, x.ArgAt<string>(0));
                    Assert.Equal(command, x.ArgAt<string>(1));
                    Assert.Equal(encoding, x.ArgAt<Encoding>(2));
                    Assert.Equal(TestContext.Current.CancellationToken, x.ArgAt<CancellationToken>(3));
                    return result.ToAsyncEnumerable(x.ArgAt<CancellationToken>(3));
                });
            _ = client.ExecuteServerEnumerableAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IAdbSocket>(), Arg.Any<Encoding>(), Arg.Any<CancellationToken>())
                .Returns(x =>
                {
                    Assert.Equal(target, x.ArgAt<string>(0));
                    Assert.Equal(command, x.ArgAt<string>(1));
                    Assert.Equal(socket, x.ArgAt<IAdbSocket>(2));
                    Assert.Equal(encoding, x.ArgAt<Encoding>(3));
                    Assert.Equal(TestContext.Current.CancellationToken, x.ArgAt<CancellationToken>(4));
                    return result.ToAsyncEnumerable(x.ArgAt<CancellationToken>(4));
                });

            await client.ExecuteServerCommandAsync(target, command, receiver, cancellationToken: TestContext.Current.CancellationToken);
            await client.ExecuteServerCommandAsync(target, command, socket, receiver, cancellationToken: TestContext.Current.CancellationToken);
            await client.ExecuteServerCommandAsync(target, command, predicate, cancellationToken: TestContext.Current.CancellationToken);
            await client.ExecuteServerCommandAsync(target, command, socket, predicate, cancellationToken: TestContext.Current.CancellationToken);
            await client.ExecuteServerCommandAsync(target, command, predicate, encoding, cancellationToken: TestContext.Current.CancellationToken);
            await client.ExecuteServerCommandAsync(target, command, socket, predicate, encoding, cancellationToken: TestContext.Current.CancellationToken);
            Assert.Equal(result, await client.ExecuteServerEnumerableAsync(target, command, cancellationToken: TestContext.Current.CancellationToken).ToListAsync(cancellationToken: TestContext.Current.CancellationToken));
            Assert.Equal(result, await client.ExecuteServerEnumerableAsync(target, command, socket, cancellationToken: TestContext.Current.CancellationToken).ToListAsync(cancellationToken: TestContext.Current.CancellationToken));
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
                    Assert.Equal(TestContext.Current.CancellationToken, x.ArgAt<CancellationToken>(4));
                    return Task.CompletedTask;
                });
            _ = client.ExecuteRemoteEnumerableAsync(Arg.Any<string>(), device, Arg.Any<Encoding>(), Arg.Any<CancellationToken>())
                .Returns(x =>
                {
                    Assert.Equal(command, x.ArgAt<string>(0));
                    Assert.Equal(device, x.ArgAt<DeviceData>(1));
                    Assert.Equal(encoding, x.ArgAt<Encoding>(2));
                    Assert.Equal(TestContext.Current.CancellationToken, x.ArgAt<CancellationToken>(3));
                    return result.ToAsyncEnumerable(x.ArgAt<CancellationToken>(3));
                });

            await client.ExecuteRemoteCommandAsync(command, device, receiver, cancellationToken: TestContext.Current.CancellationToken);
            await client.ExecuteRemoteCommandAsync(command, device, predicate, cancellationToken: TestContext.Current.CancellationToken);
            await client.ExecuteRemoteCommandAsync(command, device, predicate, encoding, cancellationToken: TestContext.Current.CancellationToken);
            Assert.Equal(result, await client.ExecuteRemoteEnumerableAsync(command, device, cancellationToken: TestContext.Current.CancellationToken).ToListAsync(cancellationToken: TestContext.Current.CancellationToken));
        }

        [Fact]
        public async Task RunLogServiceAsyncTest()
        {
            DeviceData device = new() { Serial = "169.254.109.177:5555" };
            IProgress<LogEntry> progress = Substitute.For<IProgress<LogEntry>>();
            Action<LogEntry> messageSink = progress.Report;
            LogId[] logNames = Enumerable.Range((int)LogId.Min, (int)(LogId.Max - LogId.Min + 1)).Select(x => (LogId)x).ToArray();

            CancellationToken token = TestContext.Current.CancellationToken;
            IAdbClient client = Substitute.For<IAdbClient>();
            _ = client.RunLogServiceAsync(device, Arg.Any<Action<LogEntry>>(), Arg.Any<CancellationToken>(), Arg.Any<LogId[]>())
                .Returns(x =>
                {
                    Assert.Equal(device, x.ArgAt<DeviceData>(0));
                    Assert.Equal(messageSink, x.ArgAt<Action<LogEntry>>(1));
                    Assert.Equal(token, x.ArgAt<CancellationToken>(2));
                    Assert.Equal(logNames, x.ArgAt<LogId[]>(3));
                    return Task.CompletedTask;
                });

            await client.RunLogServiceAsync(device, messageSink, token, logNames);
            await client.RunLogServiceAsync(device, progress, token, logNames);

            token = default;
            await client.RunLogServiceAsync(device, messageSink, logNames);
            await client.RunLogServiceAsync(device, progress, logNames);
        }
    }
}
