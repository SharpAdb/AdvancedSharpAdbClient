using System.IO;
using Xunit;

namespace AdvancedSharpAdbClient.Models.Tests
{
    /// <summary>
    /// Tests the <see cref="UnixFileStatus"/> class.
    /// </summary>
    public class UnixFileStatusTests
    {
        [Theory]
        [InlineData(UnixFileMode.SetUser, UnixFileStatus.SetUser)]
        [InlineData(UnixFileMode.SetGroup, UnixFileStatus.SetGroup)]
        [InlineData(UnixFileMode.StickyBit, UnixFileStatus.StickyBit)]
        [InlineData(UnixFileMode.UserRead, UnixFileStatus.UserRead)]
        [InlineData(UnixFileMode.UserWrite, UnixFileStatus.UserWrite)]
        [InlineData(UnixFileMode.UserExecute, UnixFileStatus.UserExecute)]
        [InlineData(UnixFileMode.GroupRead, UnixFileStatus.GroupRead)]
        [InlineData(UnixFileMode.GroupWrite, UnixFileStatus.GroupWrite)]
        [InlineData(UnixFileMode.GroupExecute, UnixFileStatus.GroupExecute)]
        [InlineData(UnixFileMode.OtherRead, UnixFileStatus.OtherRead)]
        [InlineData(UnixFileMode.OtherWrite, UnixFileStatus.OtherWrite)]
        [InlineData(UnixFileMode.OtherExecute, UnixFileStatus.OtherExecute)]
        public void UnixFileModeTest(UnixFileMode mode, UnixFileStatus status)
        {
            Assert.Equal((int)mode, (int)status);
            Assert.Equal(mode, (UnixFileMode)status);
            Assert.Equal((UnixFileStatus)mode, status);
        }
    }
}
