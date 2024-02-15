using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace AdvancedSharpAdbClient.Tests
{
    /// <summary>
    /// Tests the <see cref="AdbClientExtensions"/> class.
    /// </summary>
    public partial class AdbClientExtensionsTests
    {
        [Fact]
        public void ExecuteServerCommandTest()
        {
            const string target = nameof(target);
            const string command = nameof(command);
            IAdbSocket socket = Substitute.For<IAdbSocket>();
            static bool predicate(string x) => true;
            IShellOutputReceiver receiver = new FunctionOutputReceiver(predicate);
            Encoding encoding = AdbClient.Encoding;
            IEnumerable<string> result = ["Hello", "World", "!"];

            IAdbClient client = Substitute.For<IAdbClient>();
            client.When(x => x.ExecuteServerCommand(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IShellOutputReceiver>(), Arg.Any<Encoding>()))
                .Do(x =>
                {
                    Assert.Equal(target, x.ArgAt<string>(0));
                    Assert.Equal(command, x.ArgAt<string>(1));
                    Assert.Equal(receiver, x.ArgAt<IShellOutputReceiver>(2));
                    Assert.Equal(encoding, x.ArgAt<Encoding>(3));
                });
            client.When(x => x.ExecuteServerCommand(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IAdbSocket>(), Arg.Any<IShellOutputReceiver>(), Arg.Any<Encoding>()))
                .Do(x =>
                {
                    Assert.Equal(target, x.ArgAt<string>(0));
                    Assert.Equal(command, x.ArgAt<string>(1));
                    Assert.Equal(socket, x.ArgAt<IAdbSocket>(2));
                    Assert.Equal(receiver, x.ArgAt<IShellOutputReceiver>(3));
                    Assert.Equal(encoding, x.ArgAt<Encoding>(4));
                });
            _ = client.ExecuteServerCommand(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Encoding>())
                .Returns(x =>
                {
                    Assert.Equal(target, x.ArgAt<string>(0));
                    Assert.Equal(command, x.ArgAt<string>(1));
                    Assert.Equal(encoding, x.ArgAt<Encoding>(2));
                    return result;
                });
            _ = client.ExecuteServerCommand(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IAdbSocket>(), Arg.Any<Encoding>())
                .Returns(x =>
                {
                    Assert.Equal(target, x.ArgAt<string>(0));
                    Assert.Equal(command, x.ArgAt<string>(1));
                    Assert.Equal(socket, x.ArgAt<IAdbSocket>(2));
                    Assert.Equal(encoding, x.ArgAt<Encoding>(3));
                    return result;
                });

            client.ExecuteServerCommand(target, command, receiver);
            client.ExecuteServerCommand(target, command, socket, receiver);
            client.ExecuteServerCommand(target, command, predicate);
            client.ExecuteServerCommand(target, command, socket, predicate);
            client.ExecuteServerCommand(target, command, predicate, encoding);
            client.ExecuteServerCommand(target, command, socket, predicate, encoding);
            Assert.Equal(result, AdbClientExtensions.ExecuteServerCommand(client, target, command));
            Assert.Equal(result, AdbClientExtensions.ExecuteServerCommand(client, target, command, socket));
        }

        [Fact]
        public void ExecuteRemoteCommandTest()
        {
            const string command = nameof(command);
            DeviceData device = new();
            static bool predicate(string x) => true;
            IShellOutputReceiver receiver = new FunctionOutputReceiver(predicate);
            Encoding encoding = AdbClient.Encoding;
            IEnumerable<string> result = ["Hello", "World", "!"];

            IAdbClient client = Substitute.For<IAdbClient>();
            client.When(x => x.ExecuteRemoteCommand(Arg.Any<string>(), device, Arg.Any<IShellOutputReceiver>(), Arg.Any<Encoding>()))
                .Do(x =>
                {
                    Assert.Equal(command, x.ArgAt<string>(0));
                    Assert.Equal(device, x.ArgAt<DeviceData>(1));
                    Assert.Equal(receiver, x.ArgAt<IShellOutputReceiver>(2));
                    Assert.Equal(encoding, x.ArgAt<Encoding>(3));
                });
            _ = client.ExecuteRemoteCommand(Arg.Any<string>(), device, Arg.Any<Encoding>())
                .Returns(x =>
                {
                    Assert.Equal(command, x.ArgAt<string>(0));
                    Assert.Equal(device, x.ArgAt<DeviceData>(1));
                    Assert.Equal(encoding, x.ArgAt<Encoding>(2));
                    return result;
                });

            client.ExecuteRemoteCommand(command, device, receiver);
            client.ExecuteRemoteCommand(command, device, predicate);
            client.ExecuteRemoteCommand(command, device, predicate, encoding);
            Assert.Equal(result, AdbClientExtensions.ExecuteRemoteCommand(client, command, device));
        }

        [Fact]
        public void RunLogServiceTest()
        {
            DeviceData device = new();
            IProgress<LogEntry> progress = Substitute.For<IProgress<LogEntry>>();
            Action<LogEntry> messageSink = progress.Report;
            bool isCancelled = false;
            LogId[] logNames = Enumerable.Range((int)LogId.Min, (int)(LogId.Max - LogId.Min + 1)).Select(x => (LogId)x).ToArray();

            IAdbClient client = Substitute.For<IAdbClient>();
            client.When(x => x.RunLogService(device, Arg.Any<Action<LogEntry>>(), Arg.Any<bool>(), Arg.Any<LogId[]>()))
                .Do(x =>
                {
                    Assert.Equal(device, x.ArgAt<DeviceData>(0));
                    Assert.Equal(messageSink, x.ArgAt<Action<LogEntry>>(1));
                    Assert.Equal(isCancelled, x.ArgAt<bool>(2));
                    Assert.Equal(logNames, x.ArgAt<LogId[]>(3));
                });

            client.RunLogService(device, messageSink, logNames);
            client.RunLogService(device, progress, isCancelled, logNames);
            client.RunLogService(device, progress, logNames);
        }
    }
}
