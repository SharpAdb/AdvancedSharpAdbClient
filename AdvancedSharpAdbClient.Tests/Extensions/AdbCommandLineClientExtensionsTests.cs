using NSubstitute;
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
        /// <summary>
        /// Tests the <see cref="AdbCommandLineClientExtensions.EnsureIsValidAdbFile(IAdbCommandLineClient, string)"/> method.
        /// </summary>
        [Fact]
        public void EnsureIsValidAdbFileNullValueTest() =>
            _ = Assert.Throws<ArgumentNullException>(() => AdbCommandLineClientExtensions.EnsureIsValidAdbFile(null, "adb.exe"));

        /// <summary>
        /// Tests the <see cref="AdbCommandLineClientExtensions.EnsureIsValidAdbFile(IAdbCommandLineClient, string)"/> method.
        /// </summary>
        [Fact]
        public void EnsureIsValidAdbFileInvalidFileTest()
        {
            IAdbCommandLineClient clientMock = Substitute.For<IAdbCommandLineClient>();
            clientMock.IsValidAdbFile(Arg.Any<string>()).Returns(false);

            IAdbCommandLineClient client = clientMock;

            _ = Assert.Throws<FileNotFoundException>(() => client.EnsureIsValidAdbFile("xyz.exe"));
        }
    }
}
