using System;
using Xunit;

namespace AdvancedSharpAdbClient.Models.Tests
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
            _ = Assert.Throws<ArgumentOutOfRangeException>(() => ((SyncCommand)99).GetBytes());

        [Fact]
        public void SyncCommandConverterTest()
        {
            SyncCommand[] commands = Enum.GetValues<SyncCommand>();
            foreach (SyncCommand command in commands)
            {
                byte[] bytes = command.GetBytes();
                Assert.Equal(4, bytes.Length);
                Assert.Equal(command, SyncCommandConverter.GetCommand(bytes));
            }
        }
    }
}
