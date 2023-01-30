using Moq;
using System;
using System.IO;
using Xunit;

namespace AdvancedSharpAdbClient.Tests
{
    /// <summary>
    /// Tests the <see cref="AdbCommandLineClientExtensions"/> class.
    /// </summary>
    public class AdbCommandLineClientExtensionsTests
    {
        [Fact]
        public void EnsureIsValidAdbFileNullValueTest() =>
            _ = Assert.Throws<ArgumentNullException>(() => AdbCommandLineClientExtensions.EnsureIsValidAdbFile(null, "adb.exe"));

        [Fact]
        public void EnsureIsValidAdbFileInvalidFileTest()
        {
            Mock<IAdbCommandLineClient> clientMock = new();
            clientMock.Setup(c => c.IsValidAdbFile(It.IsAny<string>())).Returns(false);

            IAdbCommandLineClient client = clientMock.Object;

            _ = Assert.Throws<FileNotFoundException>(() => client.EnsureIsValidAdbFile("xyz.exe"));
        }
    }
}
