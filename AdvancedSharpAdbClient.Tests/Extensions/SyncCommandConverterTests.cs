using System;
using Xunit;

namespace AdvancedSharpAdbClient.Tests
{
    /// <summary>
    /// Tests the <see cref="SyncCommandConverter"/> class.
    /// </summary>
    public class SyncCommandConverterTests
    {
        [Fact]
        public void GetCommandNullTest() =>
            _ = Assert.Throws<ArgumentNullException>(() => SyncCommandConverter.GetCommand(null));

        [Fact]
        public void GetCommandInvalidNumberOfBytesTest() =>
            _ = Assert.Throws<ArgumentOutOfRangeException>(() => SyncCommandConverter.GetCommand([]));

        [Fact]
        public void GetCommandInvalidCommandTest() =>
            _ = Assert.Throws<ArgumentOutOfRangeException>(() => SyncCommandConverter.GetCommand("QMTV"u8));

        [Fact]
        public void GetBytesInvalidCommandTest() =>
            _ = Assert.Throws<ArgumentOutOfRangeException>(() => SyncCommandConverter.GetBytes((SyncCommand)99));

        [Fact]
        public void SyncCommandConverterTest()
        {
            SyncCommand[] commands = Enum.GetValues<SyncCommand>();
            foreach (SyncCommand command in commands)
            {
                byte[] bytes = SyncCommandConverter.GetBytes(command);
                Assert.Equal(4, bytes.Length);
                Assert.Equal(command, SyncCommandConverter.GetCommand(bytes));
            }
        }
    }
}
