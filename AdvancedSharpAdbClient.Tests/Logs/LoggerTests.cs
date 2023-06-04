using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AdvancedSharpAdbClient.Logs.Tests
{
    public class LoggerTests
    {
        [Fact]
        public async Task ReadLogTests()
        {
            using Stream stream = File.OpenRead(@"Assets/logcat.bin");
            using ShellStream shellStream = new(stream, false);
            LogReader reader = new(shellStream);

            // This stream contains 3 log entries. Read & validate the first one,
            // read the next two ones (to be sure all data is read correctly).
            LogEntry log = await reader.ReadEntry(CancellationToken.None);

            Assert.IsType<AndroidLogEntry>(log);

            Assert.Equal(707, log.ProcessId);
            Assert.Equal(1254, log.ThreadId);
            Assert.Equal(3u, log.Id);
            Assert.NotNull(log.Data);
            Assert.Equal(179, log.Data.Length);
            Assert.Equal(new DateTime(2015, 11, 14, 23, 38, 20, DateTimeKind.Utc), log.TimeStamp);

            AndroidLogEntry androidLog = (AndroidLogEntry)log;
            Assert.Equal(Priority.Info, androidLog.Priority);
            Assert.Equal("ActivityManager", androidLog.Tag);
            Assert.Equal("Start proc com.google.android.gm for broadcast com.google.android.gm/.widget.GmailWidgetProvider: pid=7026 uid=10066 gids={50066, 9997, 3003, 1028, 1015} abi=x86", androidLog.Message);

            Assert.NotNull(await reader.ReadEntry(CancellationToken.None));
            Assert.NotNull(await reader.ReadEntry(CancellationToken.None));
        }

        [Fact]
        public async Task ReadEventLogTest()
        {
            // The data in this stream was read using a ShellStream, so the CRLF fixing
            // has already taken place.
            using Stream stream = File.OpenRead(@"Assets/logcatevents.bin");
            LogReader reader = new(stream);
            LogEntry entry = await reader.ReadEntry(CancellationToken.None);

            Assert.IsType<EventLogEntry>(entry);
            Assert.Equal(707, entry.ProcessId);
            Assert.Equal(1291, entry.ThreadId);
            Assert.Equal(2u, entry.Id);
            Assert.NotNull(entry.Data);
            Assert.Equal(39, entry.Data.Length);
            Assert.Equal(new DateTime(2015, 11, 16, 1, 48, 40, DateTimeKind.Utc), entry.TimeStamp);

            EventLogEntry eventLog = (EventLogEntry)entry;
            Assert.Equal(0, eventLog.Tag);
            Assert.NotNull(eventLog.Values);
            Assert.Single(eventLog.Values);
            Assert.NotNull(eventLog.Values[0]);
            Assert.IsType<Collection<object>>(eventLog.Values[0]);

            Collection<object> list = (Collection<object>)eventLog.Values[0];
            Assert.Equal(3, list.Count);
            Assert.Equal(0, list[0]);
            Assert.Equal(19512, list[1]);
            Assert.Equal("com.amazon.kindle", list[2]);

            entry = await reader.ReadEntry(CancellationToken.None);
            entry = await reader.ReadEntry(CancellationToken.None);
        }
    }
}
